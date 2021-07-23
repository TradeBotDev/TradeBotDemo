using TradeBot.Account.AccountService.v1;

namespace Account.Validation.Messages
{
    public class IsNotEmailMessage : FailedValidationMessage
    {
        public override string Message => "Произошла ошибка: строка не является электронной почтой. Проверьте правильность введенных данных.";

        public override ActionCode Code => ActionCode.IsNotEmail;
    }
}
