using Serilog;

namespace AccountGRPC.Validation.Messages
{
    public class EmptyFieldMessage : FailedValidationMessage
    {
        public override string Message => "Произошла ошибка: присутствуют пустые поля. Проверьте правильность введенных данных.";

        public EmptyFieldMessage()
        {
            Log.Information("Ошибка валидации: присутствуют пустые поля.");
        }
    }
}
