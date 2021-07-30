using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class IsValidSessionReplies
    {
        public static IsValidSessionReply IsValid()
        {
            const string Message = "Операция является валидной.";
            Log.Information(Message);

            return new IsValidSessionReply
            {
                IsValid = true,
                Message = Message
            };
        }

        public static IsValidSessionReply IsNotValid()
        {
            const string Message = "Произошла ошибка: операция не является валидной.";
            Log.Information(Message);

            return new IsValidSessionReply
            {
                IsValid = false,
                Message = Message
            };
        }
    }
}
