using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

using TradeBot.Account.AccountService.v1;
using AccountGRPC.AccountMessages;
using AccountGRPC.Validation;
using AccountGRPC.Validation.Messages;

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

            // Проверка на существование входа в аккаунт. Если аккаунт среди вошедших не найден, отправляется
            // сообщение об ошибке.
            if (Models.State.loggedIn == null || !Models.State.loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(AddExchangeAccessReplies.AccountNotFound());

            // Валидация полученных данных. В случае, если валидация не прошла успешно, возвращается сообщение об ошибке.
            ValidationMessage validationResult = Validate.AddExchangeAccessFields(request);
            if (validationResult.Code != ActionCode.Successful)
            {
                return Task.FromResult(new AddExchangeAccessReply
                {
                    Result = validationResult.Code,
                    Message = validationResult.Message
                });
            }

            // Добавление данных из запроса в таблицу базы данных.
            using (var database = new Models.AccountContext())
            {
                var account = database.Accounts.Where(account => account.AccountId == Models.State.loggedIn[request.SessionId].AccountId);

                // Проверка на то, добавлен ли токен для той же биржи, что добавляется.
                bool isExists = database.ExchangeAccesses.Any(exchange =>
                    exchange.Account.AccountId == account.First().AccountId &&
                    exchange.Code == request.Code);

                // В случае, если данные доступа к бирже уже существуют, возвращается сообщение об этом.
                if (isExists)
                    return Task.FromResult(AddExchangeAccessReplies.ExchangeAccessExists());

                // Добавление в текущий аккаунт нового доступа к бирже.
                account.First().ExchangeAccesses.Add(new Models.ExchangeAccess
                {
                    Code = request.Code,
                    Name = request.ExchangeName,
                    Token = request.Token,
                    Secret = request.Secret,
                    Account = account.First()
                });
                database.SaveChanges();
            }
            return Task.FromResult(AddExchangeAccessReplies.SuccessfulAddition());
        }
    }
}
