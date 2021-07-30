namespace AccountGRPC.Validation.Messages
{
    public abstract class ValidationMessage
    {
        public abstract string Message { get; }

        public abstract bool Successful { get; }
    }
}
