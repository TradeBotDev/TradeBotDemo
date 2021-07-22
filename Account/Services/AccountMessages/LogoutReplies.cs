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
            Message = "Произведен выход из аккаунта."
        };

        public static readonly LogoutReply AccountNotFound = new LogoutReply
        {
            Result = ActionCode.AccountNotFound,
            Message = "Ошибка при выходе из аккаунта: вы уже вышли из аккаунта"
        };
    }
}
