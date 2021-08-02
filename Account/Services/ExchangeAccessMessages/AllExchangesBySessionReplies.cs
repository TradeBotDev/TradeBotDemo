using System.Linq;
using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class AllExchangesBySessionReplies
    {
        public static AllExchangesBySessionResponse SuccessfulGetting(IQueryable<Models.ExchangeAccess> exchangesFromAccount)
        {
            const string Message = "Получение информации о биржах завершено успешно.";
            Log.Information(Message);

            AllExchangesBySessionResponse reply = new AllExchangesBySessionResponse
            {
                Result = ExchangeAccessActionCode.Successful,
                Message = Message
            };

            foreach (Models.ExchangeAccess exchange in exchangesFromAccount)
            {
                reply.Exchanges.Add(new ExchangeAccessInfo
                {
                    ExchangeAccessId = exchange.ExchangeAccessId,
                    Code = exchange.Code,
                    Name = exchange.Name,
                    Token = exchange.Token,
                    Secret = exchange.Secret
                });
            }
            return reply;
        }

        public static AllExchangesBySessionResponse AccountNotFound()
        {
            const string Message = "Произошла ошибка получение данных бирж: пользователь не существует.";
            Log.Information(Message);

            return new AllExchangesBySessionResponse
            {
                Result = ExchangeAccessActionCode.AccountNotFound,
                Message = Message
            };
        }

        public static AllExchangesBySessionResponse ExchangesNotFound()
        {
            const string Message = "Ошибка при получении бирж: данные не найдены.";
            Log.Information(Message);

            return new AllExchangesBySessionResponse
            {
                Result = ExchangeAccessActionCode.IsNotFound,
                Message = Message
            };
        }
    }
}
