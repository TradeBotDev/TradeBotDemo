using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.AccountMessages
{
    public static class ExchangeBySessionReplies
    {
        public static ExchangeBySessionReply AccountNotFound()
        {
            const string Message = "Произошла ошибка: пользователь не найден.";
            Log.Information(Message);

            return new ExchangeBySessionReply
            {
                Result = ActionCode.AccountNotFound,
                Message = Message,
                Exchange = null
            };
        }

        public static ExchangeBySessionReply ExchangeNotFound()
        {
            const string Message = "Произошла ошибка: биржа не найдена.";
            Log.Information(Message);

            return new ExchangeBySessionReply
            {
                Result = ActionCode.ExchangeNotFound,
                Message = Message,
                Exchange = null
            };
        }

        public static ExchangeBySessionReply SuccessfulGettingExchangeAccess(Models.ExchangeAccess exchangeAccess)
        {
            const string Message = "Успешное получение информации о доступе пользователя бирже.";
            Log.Information(Message);

            return new ExchangeBySessionReply
            {
                Result = ActionCode.Successful,
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
