using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class LogoutReplies
    {
        public static LogoutResponse SuccessfulLogout()
        {
            const string Message = "Произведен выход из аккаунта.";
            Log.Information(Message);

            return new LogoutResponse
            {
                Result = AccountActionCode.Successful,
                Message = Message
            };
        }

        public static LogoutResponse AccountNotFound()
        {
            const string Message = "Ошибка при выходе из аккаунта: вы уже вышли из аккаунта";
            Log.Information(Message);

            return new LogoutResponse
            {
                Result = AccountActionCode.IsNotFound,
                Message = Message
            };
        }
    }
}
