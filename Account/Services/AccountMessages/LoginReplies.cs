using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class LoginReplies
    {
        public static LoginResponse SuccessfulLogin(string sessionId)
        {
            const string Message = "Произведен вход в аккаунт.";
            Log.Information($"{Message} SessionId: {sessionId}.");

            return new LoginResponse
            {
                SessionId = sessionId,
                Result = AccountActionCode.Successful,
                Message = Message
            };
        }

        public static LoginResponse AlreadySignedIn(string sessionId)
        {
            const string Message = "Вы уже вошли в аккаунт.";
            Log.Information($"{Message} SessionId: {sessionId}.");

            return new LoginResponse
            {
                SessionId = sessionId,
                Result = AccountActionCode.Successful,
                Message = Message
            };
        }

        public static LoginResponse AccountNotFound()
        {
            const string Message = "Ошибка при входе: пользователь не найден.";
            Log.Information(Message);

            return new LoginResponse
            {
                SessionId = "Отсутствует",
                Result = AccountActionCode.IsNotFound,
                Message = Message
            };
        }
    }
}
