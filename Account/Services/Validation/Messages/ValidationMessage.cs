using TradeBot.Account.AccountService.v1;

namespace Account.Validation.Messages
{
    public abstract class ValidationMessage
    {
        public abstract string Message { get; }

        public abstract ActionCode Code { get; }
    }
}
