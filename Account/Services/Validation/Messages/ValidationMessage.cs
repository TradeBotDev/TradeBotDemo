using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.Validation.Messages
{
    public abstract class ValidationMessage
    {
        public abstract string Message { get; }

        public abstract ValidationCode Code { get; }
    }
}
