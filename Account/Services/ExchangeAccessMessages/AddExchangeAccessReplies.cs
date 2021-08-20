using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class AddExchangeAccessReplies
    {
        public static AddExchangeAccessResponse SuccessfulAddition()
        {
            const string Message = "Добавление биржи в аккаунт пользователя завершено.";
            Log.Information(Message);

            return new AddExchangeAccessResponse
            {
                Result = ExchangeAccessActionCode.Successful,
                Message = Message
            };
        }

        public static AddExchangeAccessResponse AccountNotFound()
        {
            const string Message = "Произошла ошибка добавления биржи: пользователь не существует.";
            Log.Information(Message);

            return new AddExchangeAccessResponse
            {
                Result = ExchangeAccessActionCode.AccountNotFound,
                Message = Message
            };
        }

        public static AddExchangeAccessResponse TimePassed()
        {
            const string Message = "Произошла ошибка добавления биржи: время сессии вышло.";
            Log.Information(Message);

            return new AddExchangeAccessResponse
            {
                Result = ExchangeAccessActionCode.AccountNotFound,
                Message = Message
            };
        }

        public static AddExchangeAccessResponse ExchangeAccessExists()
        {
            const string Message = "Произошла ошибка добавления биржи: биржа уже существует.";
            Log.Information(Message);

            return new AddExchangeAccessResponse
            {
                Result = ExchangeAccessActionCode.IsExists,
                Message = Message
            };
        }

        public static AddExchangeAccessResponse LicenseNotFound()
        {
            const string Message = "Произошла ошибка добавления биржи: лицензия на данный продукт отсутствует.";
            Log.Information(Message);

            return new AddExchangeAccessResponse
            {
                Result = ExchangeAccessActionCode.LicenseNotFound,
                Message = Message
            };
        }
    }
}
