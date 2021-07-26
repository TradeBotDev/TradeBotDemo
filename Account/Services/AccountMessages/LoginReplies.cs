using Serilog;
using TradeBot.Account.AccountService.v1;

namespace Account.AccountMessages
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
                Result = ActionCode.Successful,
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
                Result = ActionCode.Successful,
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
                Result = ActionCode.AccountNotFound,
                Message = Message
            };
        }
    }
}
