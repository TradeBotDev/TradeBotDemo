using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.Validation.Messages
{
    public class SuccessfulValidationMessage : ValidationMessage
    {
        public override string Message => "Валидация завершена успешно.";

        public override ValidationCode Code => ValidationCode.Successful;

        public SuccessfulValidationMessage()
        {
            Log.Information("Успешная валидация.");
        }
    }
}
