using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace Account.AccountMessages
{
    public static class LogoutReplies
    {
        public static readonly LogoutReply SuccessfulLogout = new LogoutReply
        {
            Result = ActionCode.Successful,
            Message = "Вы вышли из аккаунта."
        };

        public static readonly LogoutReply AccountNotFound = new LogoutReply
        {
            Result = ActionCode.AccountNotFound,
            Message = "Произошла ошибка: вы уже вышли из аккаунта"
        };
    }
}
