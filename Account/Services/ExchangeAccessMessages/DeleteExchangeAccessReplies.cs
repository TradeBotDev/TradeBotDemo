using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class DeleteExchangeAccessReplies
    {
        public static DeleteExchangeAccessReply SuccessfulDeleting()
        {
            const string Message = "Данные биржи для данного пользователя успешно удалены.";
            Log.Information(Message);

            return new DeleteExchangeAccessReply
            {
                Result = ExchangeAccessActionCode.Successful,
                Message = Message
            };
        }

        public static DeleteExchangeAccessReply AccountNotFound()
        {
            const string Message = "Произошла ошибка: пользователь не найден.";
            Log.Information(Message);

            return new DeleteExchangeAccessReply
            {
                Result = ExchangeAccessActionCode.AccountNotFound,
                Message = Message
            };
        }

        public static DeleteExchangeAccessReply ExchangeNotFound()
        {
            const string Message = "Произошла ошибка: данные биржи не найдены.";
            Log.Information(Message);

            return new DeleteExchangeAccessReply
            {
                Result = ExchangeAccessActionCode.IsNotFound,
                Message = Message
            };
        }
    }
}
