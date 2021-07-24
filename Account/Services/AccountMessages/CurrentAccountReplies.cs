using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace Account.AccountMessages
{
    public static class CurrentAccountReplies
    {
        public static readonly CurrentAccountReply AccountNotFound = new CurrentAccountReply
        {
            Result = ActionCode.AccountNotFound,
            Message = "Ошибка при получении данных текущего аккаунта: пользователь не найден.",
            CurrentAccount = null
        };

        public static CurrentAccountReply SuccessfulGettingAccountData(AccountInfo currentAccount)
        {
            return new CurrentAccountReply
            {
                Result = ActionCode.Successful,
                Message = "Получение данных текущего пользователя .",
                CurrentAccount = currentAccount
            };
        }
    }
}
