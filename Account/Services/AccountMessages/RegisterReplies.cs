using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class RegisterReplies
    {
        // Логгирование.
        private static readonly ILogger logger = Log.ForContext("Where", "AccountService");

        public static RegisterResponse AccountExists()
        {
            const string Message = "Ошибка при регистрации: пользователь уже существует.";
            logger.Information(Message);

            return new RegisterResponse
            {
                Result = AccountActionCode.IsExists,
                Message = Message
            };
        }

        public static RegisterResponse SuccessfulRegister()
        {
            const string Message = "Произведена регистрация аккаунта.";
            logger.Information(Message);

            return new RegisterResponse
            {
                Result = AccountActionCode.Successful,
                Message = Message
            };
        }
    }
}
