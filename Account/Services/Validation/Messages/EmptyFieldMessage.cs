namespace AccountGRPC.Validation.Messages
{
    public class EmptyFieldMessage : FailedValidationMessage
    {
        public override string Message => "Произошла ошибка: присутствуют пустые поля. Проверьте правильность введенных данных.";

        public EmptyFieldMessage() => logger.Information("{@Class} - Ошибка валидации: присутствуют пустые поля.", GetType().Name);
    }
}
