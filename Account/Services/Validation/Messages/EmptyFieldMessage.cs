using Serilog;
using TradeBot.Account.AccountService.v1;

namespace Account.Validation.Messages
{
    public class EmptyFieldMessage : FailedValidationMessage
    {
        public override string Message => "Произошла ошибка: присутствуют пустые поля. Проверьте правильность введенных данных.";

        public override ActionCode Code => ActionCode.EmptyField;

        public EmptyFieldMessage()
        {
            Log.Information("Ошибка валидации: присутствуют пустые поля.");
        }
    }
}
