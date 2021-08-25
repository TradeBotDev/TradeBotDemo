using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class LogoutReplies
    {
        // Логгирование.
        private static readonly ILogger logger = Log.ForContext("Where", "AccountService");

        public static LogoutResponse SuccessfulLogout()
        {
            const string Message = "Произведен выход из аккаунта.";
            logger.Information("{@Replies} - " + Message, nameof(LogoutReplies));

            return new LogoutResponse
            {
                Result = AccountActionCode.Successful,
                Message = Message
            };
        }

        public static LogoutResponse AccountNotFound()
        {
            const string Message = "Ошибка при выходе из аккаунта: вы уже вышли из аккаунта";
            logger.Information("{@Replies} - " + Message, nameof(LogoutReplies));

            return new LogoutResponse
            {
                Result = AccountActionCode.IsNotFound,
                Message = Message
            };
        }
    }
}