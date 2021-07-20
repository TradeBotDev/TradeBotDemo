using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Account;
using TradeBot.Account.AccountService.v1;

namespace Account.Validation.Messages
{
    public class PasswordMismatchMessage : FailedValidationMessage
    {
        public override string Message => "Произошла ошибка: введенные пароли не совпадают. Проверьте правильность введенных данных.";

        public override ActionCode Code => ActionCode.PasswordMismatch;
    }
}
