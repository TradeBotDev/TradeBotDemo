using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class ExchangeBySessionReplies
    {
        public static ExchangeBySessionResponse AccountNotFound()
        {
            const string Message = "Произошла ошибка: пользователь не найден.";
            Log.Information(Message);

            return new ExchangeBySessionResponse
            {
                Result = ExchangeAccessActionCode.AccountNotFound,
                Message = Message,
                Exchange = null
            };
        }

        public static ExchangeBySessionResponse TimePassed()
        {
            const string Message = "Произошла ошибка: время сессии вышло.";
            Log.Information(Message);

            return new ExchangeBySessionResponse
            {
                Result = ExchangeAccessActionCode.AccountNotFound,
                Message = Message,
                Exchange = null
            };
        }

        public static ExchangeBySessionResponse ExchangeNotFound()
        {
            const string Message = "Произошла ошибка: биржа не найдена.";
            Log.Information(Message);

            return new ExchangeBySessionResponse
            {
                Result = ExchangeAccessActionCode.IsNotFound,
                Message = Message,
                Exchange = null
            };
        }

        public static ExchangeBySessionResponse SuccessfulGettingExchangeAccess(Models.ExchangeAccess exchangeAccess)
        {
            const string Message = "Успешное получение информации о доступе пользователя бирже.";
            Log.Information(Message);

            return new ExchangeBySessionResponse
            {
                Result = ExchangeAccessActionCode.Successful,
                Message = Message,
                Exchange = new ExchangeAccessInfo
                {
                    ExchangeAccessId = exchangeAccess.ExchangeAccessId,
                    Code = exchangeAccess.Code,
                    Name = exchangeAccess.Name,
                    Token = exchangeAccess.Token,
                    Secret = exchangeAccess.Secret
                }
            };
        }
    }
}
