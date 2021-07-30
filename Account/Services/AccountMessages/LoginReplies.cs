using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class LoginReplies
    {
        public static LoginReply SuccessfulLogin(string sessionId)
        {
            const string Message = "Произведен вход в аккаунт.";
            Log.Information($"{Message} SessionId: {sessionId}.");

            return new LoginReply
            {
                SessionId = sessionId,
                Result = AccountActionCode.Successful,
                Message = Message
            };
        }

        public static LoginReply AlreadySignedIn(string sessionId)
        {
            const string Message = "Вы уже вошли в аккаунт.";
            Log.Information($"{Message} SessionId: {sessionId}.");

            return new LoginReply
            {
                SessionId = sessionId,
                Result = AccountActionCode.Successful,
                Message = Message
            };
        }

        public static LoginReply AccountNotFound()
        {
            const string Message = "Ошибка при входе: пользователь не найден.";
            Log.Information(Message);

            return new LoginReply
            {
                SessionId = "Отсутствует",
                Result = AccountActionCode.IsNotFound,
                Message = Message
            };
        }
    }
}
