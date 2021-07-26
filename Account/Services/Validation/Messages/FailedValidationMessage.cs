using Serilog;

namespace Account.Validation.Messages
{
    public abstract class FailedValidationMessage : ValidationMessage
    {
        public FailedValidationMessage()
        {
            Log.Information("Ошибка валидации.");
        }
    }
}
