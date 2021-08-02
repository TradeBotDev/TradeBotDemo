using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class DeleteExchangeAccessReplies
    {
        public static DeleteExchangeAccessResponse SuccessfulDeleting()
        {
            const string Message = "Данные биржи для данного пользователя успешно удалены.";
            Log.Information(Message);

            return new DeleteExchangeAccessResponse
            {
                Result = ExchangeAccessActionCode.Successful,
                Message = Message
            };
        }

        public static DeleteExchangeAccessResponse AccountNotFound()
        {
            const string Message = "Произошла ошибка: пользователь не найден.";
            Log.Information(Message);

            return new DeleteExchangeAccessResponse
            {
                Result = ExchangeAccessActionCode.AccountNotFound,
                Message = Message
            };
        }

        public static DeleteExchangeAccessResponse ExchangeNotFound()
        {
            const string Message = "Произошла ошибка: данные биржи не найдены.";
            Log.Information(Message);

            return new DeleteExchangeAccessResponse
            {
                Result = ExchangeAccessActionCode.IsNotFound,
                Message = Message
            };
        }
    }
}
