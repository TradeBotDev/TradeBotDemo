namespace AccountGRPC.Validation.Messages
{
    public abstract class FailedValidationMessage : ValidationMessage
    {
        public override bool Successful => false;

        public FailedValidationMessage() => logger.Information("{@Class} - Ошибка валидации.", GetType().Name);
    }
}
