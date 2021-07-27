using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class AddExchangeAccessReplies
    {
        public static AddExchangeAccessReply SuccessfulAddition()
        {
            const string Message = "Добавление биржи в аккаунт пользователя завершено.";
            Log.Information(Message);

            return new AddExchangeAccessReply
            {
                Result = ActionCode.Successful,
                Message = Message
            };
        }

        public static AddExchangeAccessReply AccountNotFound()
        {
            const string Message = "Произошла ошибка добавления биржи: пользователь не существует.";
            Log.Information(Message);

            return new AddExchangeAccessReply
            {
                Result = ActionCode.AccountNotFound,
                Message = Message
            };
        }

        public static AddExchangeAccessReply ExchangeAccessExists()
        {
            const string Message = "Произошла ошибка добавления биржи: биржа уже существует.";
            Log.Information(Message);

            return new AddExchangeAccessReply
            {
                Result = ActionCode.ExchangeExists,
                Message = Message
            };
        }
    }
}
