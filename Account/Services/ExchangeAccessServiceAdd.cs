using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

using TradeBot.Account.AccountService.v1;
using AccountGRPC.AccountMessages;
using AccountGRPC.Validation;
using AccountGRPC.Validation.Messages;
using Microsoft.EntityFrameworkCore;

namespace AccountGRPC
{
    public partial class ExchangeAccessService : ExchangeAccess.ExchangeAccessBase
    {
        // Метод, добавляющий новую биржу для текущего пользователя.
        public override Task<AddExchangeAccessReply> AddExchangeAccess(AddExchangeAccessRequest request, ServerCallContext context)
        {
            Log.Information($"AddExchangeAccess получил запрос: " +
                $"SessionId - {request.SessionId}, " +
                $"Code - {request.Code}, " +
                $"ExchangeName - {request.ExchangeName}, " +
                $"Token - {request.Token}, " +
                $"Secret - {request.Secret}.");

            using (var database = new Models.AccountContext())
            {
                // Проверка на существование входа в аккаунт. Если аккаунт среди вошедших не найден, отправляется
                // сообщение об ошибке.
                if (!database.LoggedAccounts.Any(login => login.SessionId == request.SessionId))
                    return Task.FromResult(AddExchangeAccessReplies.AccountNotFound());

                // Валидация полученных данных. В случае, если валидация не прошла успешно, возвращается сообщение об ошибке.
                ValidationMessage validationResult = Validate.AddExchangeAccessFields(request);
                if (!validationResult.Successful)
                {
                    return Task.FromResult(new AddExchangeAccessReply
                    {
                        Result = ExchangeAccessActionCode.Failed,
                        Message = validationResult.Message
                    });
                }

                // Получение данных о текущем входе (которому соответствует Id сессии) и информации об аккаунте.
                var loginInfo = database.LoggedAccounts
                    .Where(login => login.SessionId == request.SessionId)
                    .Include(account => account.Account).First();

                // Проверка на то, была ли уже добавлена информация о добавляемой бирже.
                bool isExists = database.ExchangeAccesses.Any(exchange =>
                    exchange.Account.AccountId == loginInfo.Account.AccountId &&
                    exchange.Code == request.Code);

                // В случае, если данные доступа к бирже уже существуют, возвращается сообщение об этом.
                if (isExists)
                    return Task.FromResult(AddExchangeAccessReplies.ExchangeAccessExists());

                // Добавление в текущий аккаунт нового доступа к бирже.
                loginInfo.Account.ExchangeAccesses.Add(new Models.ExchangeAccess
                {
                    Code = request.Code,
                    Name = request.ExchangeName,
                    Token = request.Token,
                    Secret = request.Secret,
                    Account = loginInfo.Account
                });
                database.SaveChanges();
            }
            return Task.FromResult(AddExchangeAccessReplies.SuccessfulAddition());
        }
    }
}
