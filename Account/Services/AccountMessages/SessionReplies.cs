using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace Account.AccountMessages
{
    public static class SessionReplies
    {
        public static readonly SessionReply IsValid = new SessionReply
        {
            IsValid = true,
            Message = "Операция является валидной."
        };

        public static readonly SessionReply IsNotValid = new SessionReply
        {
            IsValid = false,
            Message = "Произошла ошибка: операция не является валидной."
        };
    }
}
