using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

using TradeBot.Account.AccountService.v1;
using AccountGRPC.Validation;
using AccountGRPC.Validation.Messages;
using AccountGRPC.AccountMessages;

namespace AccountGRPC
{
    public partial class AccountService : Account.AccountBase
    {
        // Метод входа в аккаунт по запросу клиента.
        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            Log.Information($"Login получил запрос: Email - {request.Email}, Password - {request.Password}, SaveExchangesAfterLogout - {request.SaveExchangesAfterLogout}.");

            // Валидация полей запроса
            ValidationMessage validationResult = Validate.LoginFields(request);

            // В случае, если валидация не прошла успешно (к примеру, присутствуют пустые поля)
            // возвращается сообщение об одной из ошибок в запросе.
            if (validationResult.Code != ActionCode.Successful)
            {
                return Task.FromResult(new LoginReply
                {
                    SessionId = "none",
                    Result = validationResult.Code,
                    Message = validationResult.Message
                });
            }

            // В случае успешной прохождении валидации используется база данных.
            using (var database = new Models.AccountContext())
            {
                // Получение всех пользователей с таким же Email-адресом и паролем, как в запросе.
                var accounts = database.Accounts.Where(accounts => accounts.Email == request.Email &&
                    accounts.Password == request.Password);

                // Проверка на наличие зарегистрированных аккаунтов с данными из запроса, и в
                // случае их отсутствия отправляет ответ с сообщением об ошибке.
                if (accounts.Count() == 0)
                    return Task.FromResult(LoginReplies.AccountNotFound());

                // Проверка на то, есть ли сессия с пользователем, который пытается войти в аккаунт, и
                // в случае, если он вошел, возвращается его Id сессии
                foreach (KeyValuePair<string, Models.LoggedAccount> account in Models.State.loggedIn)
                {
                    int checkedAccount = database.Accounts.Count(account => account.Email == request.Email);
                    if (checkedAccount > 0)
                        return Task.FromResult(LoginReplies.AlreadySignedIn(account.Key));
                }

                // В случае наличия зарегистрированного аккаунта с данными из запроса генерируется
                // Id сессии, а также полученный пользователь добавляется в коллекцию с вошедшими
                // пользователями.
                string sessionId = Guid.NewGuid().ToString();
                var loggedAccount = new Models.LoggedAccount(accounts.First(), request.SaveExchangesAfterLogout);
                Models.State.loggedIn.Add(sessionId, loggedAccount);

                // Сохранение текущего состояния в файл.
                FileManagement.WriteFile(Models.State.LoggedInFilename, Models.State.loggedIn);

                // Ответ сервера об успешном входе в аккаунт.
                return Task.FromResult(LoginReplies.SuccessfulLogin(sessionId));
            }
        }
    }
}
