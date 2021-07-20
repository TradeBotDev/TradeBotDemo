using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Account.Validation;
using Account.Validation.Messages;
using Account.AccountMessages;

namespace Account
{
    public partial class AccountService : TradeBot.Account.AccountService.v1.Account.AccountBase
    {
        // Метод входа в аккаунт по запросу клиента.
        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
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
                    return Task.FromResult(LoginReplies.AccountNotFound);

                // Проверка на то, есть ли сессия с пользователем, который пытается войти в аккаунт, и
                // в случае, если он вошел, возвращается его Id сессии
                foreach (KeyValuePair<string, Models.Account> account in loggedIn)
                {
                    if (request.Email == account.Value.Email)
                        return Task.FromResult(LoginReplies.AlreadySignedIn(account.Key));
                }

                // В случае наличия зарегистрированного аккаунта с данными из запроса генерируется
                // Id сессии, а также полученный пользователь добавляется в коллекцию с вошедшими
                // пользователями.
                string sessionId = Guid.NewGuid().ToString();
                loggedIn.Add(sessionId, accounts.First());

                // Сохранение текущего состояния в файл.
                FileManagement.WriteState(loggedInFilename, loggedIn);

                // Ответ сервера об успешном входе в аккаунт.
                return Task.FromResult(LoginReplies.SuccessfulLogin(sessionId));
            }
        }
    }
}
