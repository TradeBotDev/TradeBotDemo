using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class IsValidSessionReplies
    {
        public static IsValidSessionResponse IsValid()
        {
            const string Message = "Операция является валидной.";
            Log.Information(Message);

            return new IsValidSessionResponse
            {
                IsValid = true,
                Message = Message
            };
        }

        public static IsValidSessionResponse IsNotValid()
        {
            const string Message = "Произошла ошибка: операция не является валидной.";
            Log.Information(Message);

            return new IsValidSessionResponse
            {
                IsValid = false,
                Message = Message
            };
        }
    }
}
