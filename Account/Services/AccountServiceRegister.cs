using Grpc.Core;
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
        // Метод регистрации аккаунта по запросу клиента. Вход в аккаунт после регистрации НЕ производится!
        public override Task<RegisterReply> Register(RegisterRequest request, ServerCallContext context)
        {
            Log.Information($"Login получил запрос: Email - {request.Email}, Password - {request.Password}, VerifyPassword - {request.VerifyPassword}.");

            // Валидация полей запроса
            ValidationMessage validationResult = Validate.RegisterFields(request);

            // В случае, если валидация не прошла успешно (к примеру, присутствуют пустые поля)
            // возвращается сообщение об одной из ошибок в запросе.
            if (validationResult.Code != ActionCode.Successful)
            {
                return Task.FromResult(new RegisterReply
                {
                    Result = validationResult.Code,
                    Message = validationResult.Message
                });
            }

            // В случае успешной прохождении валидации используется база данных.
            using (var database = new Models.AccountContext()) {
                // Получение всех пользователей с таким же Email-адресом и паролем, как в запросе.
                var accountsWithThisEmail = database.Accounts.Where(accounts => accounts.Email == request.Email);

                // В случае наличия аккаунтов с таким же Email-адресом, как в запросе, возвращается
                // ответ сервера с ошибкой, сообщающей об этом.
                if (accountsWithThisEmail.Count() > 0)
                    return Task.FromResult(RegisterReplies.AccountExists());

                // В случае отсутствия пользователей с тем же Email-адресом, добавление в базу данных
                // нового пользователя с данными из базы данных.
                database.Add(new Models.Account()
                {
                    Email = request.Email,
                    Password = request.Password
                });
                // Сохранение изменений базы данных и возвращение ответа.
                database.SaveChanges();
                return Task.FromResult(RegisterReplies.SuccessfulRegister());
            }
        }
    }
}
