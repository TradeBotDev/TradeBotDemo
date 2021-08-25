namespace AccountGRPC.Validation.Messages
{
    public class PasswordMismatchMessage : FailedValidationMessage
    {
        public override string Message => "Произошла ошибка: введенные пароли не совпадают. Проверьте правильность введенных данных.";

        public PasswordMismatchMessage() => logger.Information("{@Class} - Ошибка валидации: введенные пароли не совпадают.", GetType().Name);
    }
}
