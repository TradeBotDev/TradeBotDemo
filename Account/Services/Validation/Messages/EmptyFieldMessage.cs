using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.Validation.Messages
{
    public class EmptyFieldMessage : FailedValidationMessage
    {
        public override string Message => "Произошла ошибка: присутствуют пустые поля. Проверьте правильность введенных данных.";

        public override ValidationCode Code => ValidationCode.EmptyField;

        public EmptyFieldMessage()
        {
            Log.Information("Ошибка валидации: присутствуют пустые поля.");
        }
    }
}
