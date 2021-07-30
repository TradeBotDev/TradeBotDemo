using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

using TradeBot.Account.AccountService.v1;
using AccountGRPC.AccountMessages;
using Microsoft.EntityFrameworkCore;

namespace AccountGRPC
{
    public partial class ExchangeAccessService : ExchangeAccess.ExchangeAccessBase
    {
        public override Task<AllExchangesBySessionReply> AllExchangesBySession(SessionRequest request, ServerCallContext context)
        {
            Log.Information($"AllExchangesBySession получил запрос: SessionId - {request.SessionId}.");
            using (var database = new Models.AccountContext())
            {
                // Получение данных о входе по текущему Id сессии.
                var login = database.LoggedAccounts
                    .Where(login => login.SessionId == request.SessionId)
                    .Include(account => account.Account);

                // В случае, если аккаунт не найден среди вошедших, возвращается сообщение об ошибке.
                if (login.Count() == 0)
                    return Task.FromResult(AllExchangesBySessionReplies.AccountNotFound());

                // Поиск информации о доступе пользователя к бирже.
                var exchangeAccesses = database.ExchangeAccesses
                    .Include(account => account.Account)
                    .Where(exchange => exchange.Account.AccountId == login.First().Account.AccountId);

                // Ошибка в случае, если биржи не найдены.
                if (exchangeAccesses.Count() == 0)
                    return Task.FromResult(AllExchangesBySessionReplies.ExchangesNotFound());

                // Формирование ответа со всей необходимой информацией о добавленных биржах.
                var reply = AllExchangesBySessionReplies.SuccessfulGetting(exchangeAccesses);
                return Task.FromResult(reply);
            }
        }

        // Получение данных конкретной биржи пользователя.
        public override Task<ExchangeBySessionReply> ExchangeBySession(ExchangeBySessionRequest request, ServerCallContext context)
        {
            Log.Information($"ExchangeBySession получил запрос: SessionId - {request.SessionId}, Code - {request.Code}.");
            using (var database = new Models.AccountContext())
            {
                // Получение данных о входе по текущему Id сессии.
                var login = database.LoggedAccounts
                    .Where(login => login.SessionId == request.SessionId)
                    .Include(account => account.Account);

                // В случае, если аккаунт не найден среди вошедших, возвращается сообщение об ошибке.
                if (login.Count() == 0)
                    return Task.FromResult(ExchangeBySessionReplies.AccountNotFound());

                // Поиск информации о доступе пользователя к бирже.
                var exchangeAccesses = database.ExchangeAccesses
                    .Include(account => account.Account)
                    .Where(exchange => exchange.Code == request.Code &&
                        exchange.Account.AccountId == login.First().Account.AccountId);

                // Если такой биржи не было найдено, возвращается сообщение об ошибке.
                if (exchangeAccesses.Count() == 0)
                    return Task.FromResult(ExchangeBySessionReplies.ExchangeNotFound());

                // В случае успешного получения доступа к бирже она возвращается в качестве ответа.
                var exchangeAccess = exchangeAccesses.First();
                return Task.FromResult(ExchangeBySessionReplies.SuccessfulGettingExchangeAccess(exchangeAccess));
            }
        }
    }
}
