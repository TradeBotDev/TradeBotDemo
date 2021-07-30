using Serilog;

namespace AccountGRPC.Validation.Messages
{
    public abstract class FailedValidationMessage : ValidationMessage
    {
        public override bool Successful => false;

        public FailedValidationMessage()
        {
            Log.Information("Ошибка валидации.");
        }
    }
}
