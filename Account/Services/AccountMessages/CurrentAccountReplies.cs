﻿using System;
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
            Message = "Произошла ошибка: пользователь не найден.",
            CurrentAccount = null
        };

        public static CurrentAccountReply SuccessfulOperation(AccountInfo currentAccount)
        {
            return new CurrentAccountReply
            {
                Result = ActionCode.Successful,
                Message = "Операция завершена успешно.",
                CurrentAccount = currentAccount
            };
        }
    }
}