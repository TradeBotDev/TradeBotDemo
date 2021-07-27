using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class LogoutReplies
    {
        public static LogoutReply SuccessfulLogout()
        {
            const string Message = "Произведен выход из аккаунта.";
            Log.Information(Message);

            return new LogoutReply
            {
                Result = ActionCode.Successful,
                Message = Message
            };
        }

        public static LogoutReply AccountNotFound()
        {
            const string Message = "Ошибка при выходе из аккаунта: вы уже вышли из аккаунта";
            Log.Information(Message);

            return new LogoutReply
            {
                Result = ActionCode.AccountNotFound,
                Message = Message
            };
        }
    }
}
