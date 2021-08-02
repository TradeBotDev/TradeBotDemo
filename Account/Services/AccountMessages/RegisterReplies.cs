using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class RegisterReplies
    {
        public static RegisterResponse AccountExists()
        {
            const string Message = "Ошибка при регистрации: пользователь уже существует.";
            Log.Information(Message);

            return new RegisterResponse
            {
                Result = AccountActionCode.IsExists,
                Message = Message
            };
        }

        public static RegisterResponse SuccessfulRegister()
        {
            const string Message = "Произведена регистрация аккаунта.";
            Log.Information(Message);

            return new RegisterResponse
            {
                Result = AccountActionCode.Successful,
                Message = Message
            };
        }
    }
}
