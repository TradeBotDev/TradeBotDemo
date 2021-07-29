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
        public override Task<AllExchangesBySessionReply> AllExchangesBySession(SessionRequest request, ServerCallContext context)
        {
            Log.Information($"AllExchangesBySession получил запрос: SessionId - {request.SessionId}.");

            if (Models.State.loggedIn == null || !Models.State.loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(AllExchangesBySessionReplies.AccountNotFound());
            else
            {
                using (var database = new Models.AccountContext())
                {
                    // Получение данных текущей сессии.
                    Models.LoggedAccount fromAccount = Models.State.loggedIn[request.SessionId];
                    // Поиск всех добавленных бирж по id пользователя текущей сессии.
                    var exchangesFromAccount = database.ExchangeAccesses.Where(exchange => exchange.Account.AccountId == fromAccount.AccountId);

                    // Ошибка в случае, если биржи не найдены.
                    if (exchangesFromAccount.Count() == 0)
                        return Task.FromResult(AllExchangesBySessionReplies.ExchangesNotFound());

                    // Формирование ответа со всей необходимой информацией о добавленных биржах.
                    var reply = AllExchangesBySessionReplies.SuccessfulGetting(exchangesFromAccount);
                    return Task.FromResult(reply);
                }
            }
        }

        // Получение данных конкретной биржи пользователя.
        public override Task<ExchangeBySessionReply> ExchangeBySession(ExchangeBySessionRequest request, ServerCallContext context)
        {
            Log.Information($"ExchangeBySession получил запрос: SessionId - {request.SessionId}, Code - {request.Code}.");

            // В случае, если аккаунт не найден среди вошедших, возвращается сообщение об ошибке.
            if (Models.State.loggedIn == null || !Models.State.loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(ExchangeBySessionReplies.AccountNotFound());

            using (var database = new Models.AccountContext())
            {
                // Поиск данных конкретной биржи пользователя.
                var exchangeAccesses = database.ExchangeAccesses.Where(exchange => exchange.Code == request.Code &&
                    exchange.Account.AccountId == Models.State.loggedIn[request.SessionId].AccountId);

                // Если такой биржи не было найдено, возвращается сообшение об ошибке.
                if (exchangeAccesses.Count() == 0)
                    return Task.FromResult(ExchangeBySessionReplies.ExchangeNotFound());

                // В случае успешного получения доступа к бирже она возвращается в качестве ответа.
                var exchangeAccess = exchangeAccesses.First();
                return Task.FromResult(ExchangeBySessionReplies.SuccessfulGettingExchangeAccess(exchangeAccess));
            }
        }
    }
}
