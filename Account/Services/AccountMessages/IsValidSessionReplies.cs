using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class IsValidSessionReplies
    {
        // Логгирование.
        private static readonly ILogger logger = Log.ForContext("Where", "AccountService");

        public static IsValidSessionResponse IsValid()
        {
            const string Message = "Операция является валидной.";
            logger.Information("{@Replies} - " + Message, nameof(IsValidSessionReplies));

            return new IsValidSessionResponse
            {
                IsValid = true,
                Message = Message
            };
        }

        public static IsValidSessionResponse IsNotValid()
        {
            const string Message = "Произошла ошибка: операция не является валидной.";
            logger.Information("{@Replies} - " + Message, nameof(IsValidSessionReplies));

            return new IsValidSessionResponse
            {
                IsValid = false,
                Message = Message
            };
        }
    }
}