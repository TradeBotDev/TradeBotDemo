using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class DeleteExchangeAccessReplies
    {
        // Логгирование.
        private static readonly ILogger logger = Log.ForContext("Where", "AccountService");

        public static DeleteExchangeAccessResponse SuccessfulDeleting()
        {
            const string Message = "Данные биржи для данного пользователя успешно удалены.";
            logger.Information("{@Replies} - " + Message, nameof(DeleteExchangeAccessReplies));

            return new DeleteExchangeAccessResponse
            {
                Result = ExchangeAccessActionCode.Successful,
                Message = Message
            };
        }

        public static DeleteExchangeAccessResponse AccountNotFound()
        {
            const string Message = "Произошла ошибка: пользователь не найден.";
            logger.Information("{@Replies} - " + Message, nameof(DeleteExchangeAccessReplies));

            return new DeleteExchangeAccessResponse
            {
                Result = ExchangeAccessActionCode.AccountNotFound,
                Message = Message
            };
        }

        public static DeleteExchangeAccessResponse TimePassed()
        {
            const string Message = "Произошла ошибка: время сессии вышло.";
            logger.Information("{@Replies} - " + Message, nameof(DeleteExchangeAccessReplies));

            return new DeleteExchangeAccessResponse
            {
                Result = ExchangeAccessActionCode.AccountNotFound,
                Message = Message
            };
        }

        public static DeleteExchangeAccessResponse ExchangeNotFound()
        {
            const string Message = "Произошла ошибка: данные биржи не найдены.";
            logger.Information("{@Replies} - " + Message, nameof(DeleteExchangeAccessReplies));

            return new DeleteExchangeAccessResponse
            {
                Result = ExchangeAccessActionCode.IsNotFound,
                Message = Message
            };
        }
    }
}
