using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

using TradeBot.Account.AccountService.v1;
using AccountGRPC.AccountMessages;

namespace AccountGRPC
{
    public partial class ExchangeAccessService : ExchangeAccess.ExchangeAccessBase
    {
        // Метод, удаляющий данные одной из бирж для текущего пользователя по id записи.
        public override Task<DeleteExchangeAccessReply> DeleteExchangeAccess(DeleteExchangeAccessRequest request, ServerCallContext context)
        {
            Log.Information($"DeleteExchangeAccess получил запрос: SessionId - {request.SessionId}, Code - {request.Code}.");

            using (var database = new Models.AccountContext())
            {
                if (!database.LoggedAccounts.Any(login => login.SessionId == request.SessionId))
                    return Task.FromResult(DeleteExchangeAccessReplies.AccountNotFound());

                Models.LoggedAccount fromAccount = database.LoggedAccounts.Where(login => login.SessionId == request.SessionId).First();

                // Получение данных биржи для текущего пользователя и биржи с конкретным кодом.
                var exсhangeAccess = database.ExchangeAccesses.Where(exchange =>
                    exchange.Account.AccountId == fromAccount.Account.AccountId &&
                    exchange.Code == request.Code);

                // В случае, если такой записи не обнаружено, сервис отвечает ошибкой.
                if (exсhangeAccess.Count() == 0)
                    return Task.FromResult(DeleteExchangeAccessReplies.ExchangeNotFound());

                // Если такая запись существует, производится ее удаление.
                database.ExchangeAccesses.Remove(exсhangeAccess.First());
                database.SaveChanges();
            }
            return Task.FromResult(DeleteExchangeAccessReplies.SuccessfulDeleting());
        }
    }
}
