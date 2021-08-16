using Serilog;

namespace AccountGRPC.Validation.Messages
{
    public class PasswordMismatchMessage : FailedValidationMessage
    {
        public override string Message => "Произошла ошибка: введенные пароли не совпадают. Проверьте правильность введенных данных.";

        public PasswordMismatchMessage()
        {
            Log.Information("Ошибка валидации: введенные пароли не совпадают.");
        }
    }
}
