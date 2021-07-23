using TradeBot.Account.AccountService.v1;

namespace Account.AccountMessages
{
    public static class ExchangeBySessionReplies
    {
        public static readonly ExchangeBySessionReply AccountNotFound = new ExchangeBySessionReply
        {
            Result = ActionCode.AccountNotFound,
            Message = "Произошла ошибка: пользователь не найден",
            Exchange = null
        };

        public static readonly ExchangeBySessionReply ExchangeNotFound = new ExchangeBySessionReply
        {
            Result = ActionCode.ExchangeNotFound,
            Message = "Произошла ошибка: биржа не найдена",
            Exchange = null
        };

        public static ExchangeBySessionReply SuccessfulGettingExchangeAccess(Models.ExchangeAccess exchangeAccess)
        {
            return new ExchangeBySessionReply
            {
                Result = ActionCode.Successful,
                Message = "Успешно",
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
