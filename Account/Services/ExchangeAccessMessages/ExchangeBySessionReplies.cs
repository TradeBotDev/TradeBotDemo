using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class ExchangeBySessionReplies
    {
        // Логгирование.
        private static readonly ILogger logger = Log.ForContext("Where", "AccountService");

        public static ExchangeBySessionResponse AccountNotFound()
        {
            const string Message = "Произошла ошибка: пользователь не найден.";
            logger.Information("{@Replies} - " + Message, nameof(ExchangeBySessionReplies));

            return new ExchangeBySessionResponse
            {
                Result = ExchangeAccessActionCode.AccountNotFound,
                Message = Message,
                Exchange = null
            };
        }

        public static ExchangeBySessionResponse TimePassed()
        {
            const string Message = "Произошла ошибка: время сессии вышло.";
            logger.Information("{@Replies} - " + Message, nameof(ExchangeBySessionReplies));

            return new ExchangeBySessionResponse
            {
                Result = ExchangeAccessActionCode.AccountNotFound,
                Message = Message,
                Exchange = null
            };
        }

        public static ExchangeBySessionResponse ExchangeNotFound()
        {
            const string Message = "Произошла ошибка: биржа не найдена.";
            logger.Information("{@Replies} - " + Message, nameof(ExchangeBySessionReplies));

            return new ExchangeBySessionResponse
            {
                Result = ExchangeAccessActionCode.IsNotFound,
                Message = Message,
                Exchange = null
            };
        }

        public static ExchangeBySessionResponse SuccessfulGettingExchangeAccess(Models.ExchangeAccess exchangeAccess)
        {
            const string Message = "Успешное получение информации о доступе пользователя бирже.";
            logger.Information("{@Replies} - " + Message, nameof(ExchangeBySessionReplies));

            return new ExchangeBySessionResponse
            {
                Result = ExchangeAccessActionCode.Successful,
                Message = Message,
                Exchange = new ExchangeAccessInfo
                {
                    ExchangeAccessId = exchangeAccess.ExchangeAccessId,
                    Code = exchangeAccess.Code,
                    Name = exchangeAccess.Name,
                    Token = exchangeAccess.Token,
                    Secret = exchangeAccess.Secret
                }
            };
        }
    }
}
