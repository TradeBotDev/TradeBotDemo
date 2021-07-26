using Serilog;
using TradeBot.Account.AccountService.v1;

namespace Account.AccountMessages
{
    public static class IsValidSessionReplies
    {
        public static SessionReply IsValid()
        {
            const string Message = "Операция является валидной.";
            Log.Information(Message);

            return new SessionReply
            {
                IsValid = true,
                Message = Message
            };
        }

        public static SessionReply IsNotValid()
        {
            const string Message = "Произошла ошибка: операция не является валидной.";
            Log.Information(Message);

            return new SessionReply
            {
                IsValid = false,
                Message = Message
            };
        }
    }
}
