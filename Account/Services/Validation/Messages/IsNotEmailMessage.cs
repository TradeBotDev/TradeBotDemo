﻿using Serilog;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.Validation.Messages
{
    public class IsNotEmailMessage : FailedValidationMessage
    {
        public override string Message => "Произошла ошибка: строка не является электронной почтой. Проверьте правильность введенных данных.";

        public override ActionCode Code => ActionCode.IsNotEmail;

        public IsNotEmailMessage()
        {
            Log.Information("Ошибка валидации: строка не является электронной почтой.");
        }
    }
}
