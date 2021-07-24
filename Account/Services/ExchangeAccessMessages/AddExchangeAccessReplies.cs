using TradeBot.Account.AccountService.v1;

namespace Account.AccountMessages
{
    public static class AddExchangeAccessReplies
    {
        public static readonly AddExchangeAccessReply SuccessfulAddition = new AddExchangeAccessReply
        {
            Result = ActionCode.Successful,
            Message = "Добавление биржи в аккаунт пользователя завершено."
        };

        public static readonly AddExchangeAccessReply AccountNotFound = new AddExchangeAccessReply
        {
            Result = ActionCode.AccountNotFound,
            Message = "Произошла ошибка добавления биржи: пользователь не существует."
        };

        public static readonly AddExchangeAccessReply ExchangeAccessExists = new AddExchangeAccessReply
        {
            Result = ActionCode.ExchangeExists,
            Message = "Произошла ошибка добавления биржи: биржа уже существует."
        };
    }
}
