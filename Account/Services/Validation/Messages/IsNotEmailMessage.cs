﻿namespace AccountGRPC.Validation.Messages
{
    public class IsNotEmailMessage : FailedValidationMessage
    {
        public override string Message => "Произошла ошибка: строка не является электронной почтой. Проверьте правильность введенных данных.";

        public IsNotEmailMessage() => logger.Information("{@Class} - Ошибка валидации: строка не является электронной почтой.", GetType().Name);
    }
}
