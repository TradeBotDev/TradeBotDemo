using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class LoginReplies
    {
        // Логгирование.
        private static readonly ILogger logger = Log.ForContext("Where", "AccountService");

        public static LoginResponse SuccessfulLogin(string sessionId, int accountId)
        {
            const string Message = "Произведен вход в аккаунт.";
            logger.Information("{@Replies} - " + $"{Message} SessionId: {sessionId}", nameof(LoginReplies));

            return new LoginResponse
            {
                SessionId = sessionId,
                AccountId = accountId,
                Result = AccountActionCode.Successful,
                Message = Message
            };
        }

        public static LoginResponse AlreadySignedIn(string sessionId, int accountId)
        {
            const string Message = "Вы уже вошли в аккаунт.";
            logger.Information("{@Replies} - " + $"{Message} SessionId: {sessionId}", nameof(LoginReplies));

            return new LoginResponse
            {
                SessionId = sessionId,
                AccountId = accountId,
                Result = AccountActionCode.Successful,
                Message = Message
            };
        }

        public static LoginResponse AccountNotFound()
        {
            const string Message = "Ошибка при входе: пользователь не найден.";
            logger.Information("{@Replies} - " + Message, nameof(LoginReplies));

            return new LoginResponse
            {
                SessionId = "Отсутствует",
                Result = AccountActionCode.IsNotFound,
                Message = Message
            };
        }
    }
}