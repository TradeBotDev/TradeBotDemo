using Serilog;

namespace AccountGRPC.Validation.Messages
{
    public class SuccessfulValidationMessage : ValidationMessage
    {
        public override string Message => "Валидация завершена успешно.";

        public override bool Successful => true;

        public SuccessfulValidationMessage()
        {
            Log.Information("Успешная валидация.");
        }
    }
}
