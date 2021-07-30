using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class RegisterReplies
    {
        public static RegisterReply AccountExists()
        {
            const string Message = "Ошибка при регистрации: пользователь уже существует.";
            Log.Information(Message);

            return new RegisterReply
            {
                Result = AccountActionCode.IsExists,
                Message = Message
            };
        }

        public static RegisterReply SuccessfulRegister()
        {
            const string Message = "Произведена регистрация аккаунта.";
            Log.Information(Message);

            return new RegisterReply
            {
                Result = AccountActionCode.Successful,
                Message = Message
            };
        }
    }
}
