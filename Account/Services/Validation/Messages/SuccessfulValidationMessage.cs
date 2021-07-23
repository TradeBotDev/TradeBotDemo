﻿using TradeBot.Account.AccountService.v1;

namespace Account.Validation.Messages
{
    public class SuccessfulValidationMessage : ValidationMessage
    {
        public override string Message => "Валидация завершена успешно.";

        public override ActionCode Code => ActionCode.Successful;
    }
}