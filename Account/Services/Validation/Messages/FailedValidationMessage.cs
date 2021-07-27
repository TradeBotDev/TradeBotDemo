using Serilog;

namespace AccountGRPC.Validation.Messages
{
    public abstract class FailedValidationMessage : ValidationMessage
    {
        public FailedValidationMessage()
        {
            Log.Information("Ошибка валидации.");
        }
    }
}
