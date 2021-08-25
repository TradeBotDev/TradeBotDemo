using Serilog;

namespace AccountGRPC.Validation.Messages
{
    public abstract class ValidationMessage
    {
        protected readonly ILogger logger = Log.ForContext("Where", "AccountService");

        public abstract string Message { get; }

        public abstract bool Successful { get; }
    }
}
