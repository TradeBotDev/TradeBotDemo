using TradeBot.Account.AccountService.v1;

namespace Account.AccountMessages
{
    public static class DeleteExchangeAccessReplies
    {
        public static readonly DeleteExchangeAccessReply SuccessfulDeleting = new DeleteExchangeAccessReply
        {
            Result = ActionCode.Successful,
            Message = "Данные биржи для данного пользователя успешно удалены."
        };

        public static readonly DeleteExchangeAccessReply AccountNotFound = new DeleteExchangeAccessReply
        {
            Result = ActionCode.ExchangeNotFound,
            Message = "Произошла ошибка: пользователь не найдены."
        };

        public static readonly DeleteExchangeAccessReply ExchangeNotFound = new DeleteExchangeAccessReply
        {
            Result = ActionCode.ExchangeNotFound,
            Message = "Произошла ошибка: данные биржи не найдены."
        };
    }
}
