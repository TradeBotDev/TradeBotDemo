using System.Linq;
using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class AllExchangesBySessionReplies
    {
        // Логгирование.
        private static readonly ILogger logger = Log.ForContext("Where", "AccountService");

        public static AllExchangesBySessionResponse SuccessfulGetting(IQueryable<Models.ExchangeAccess> exchangesFromAccount)
        {
            const string Message = "Получение информации о биржах завершено успешно.";
            logger.Information("{@Replies} - " + Message, nameof(AllExchangesBySessionReplies));

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
            logger.Information("{@Replies} - " + Message, nameof(AllExchangesBySessionReplies));

            return new AllExchangesBySessionResponse
            {
                Result = ExchangeAccessActionCode.AccountNotFound,
                Message = Message
            };
        }

        public static AllExchangesBySessionResponse TimePassed()
        {
            const string Message = "Произошла ошибка получение данных бирж: время сессии вышло.";
            logger.Information("{@Replies} - " + Message, nameof(AllExchangesBySessionReplies));

            return new AllExchangesBySessionResponse
            {
                Result = ExchangeAccessActionCode.AccountNotFound,
                Message = Message
            };
        }

        public static AllExchangesBySessionResponse ExchangesNotFound()
        {
            const string Message = "Ошибка при получении бирж: данные не найдены.";
            logger.Information("{@Replies} - " + Message, nameof(AllExchangesBySessionReplies));

            return new AllExchangesBySessionResponse
            {
                Result = ExchangeAccessActionCode.IsNotFound,
                Message = Message
            };
        }
    }
}
