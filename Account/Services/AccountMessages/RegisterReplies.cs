using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace Account.AccountMessages
{
    public static class RegisterReplies
    {
        public static readonly RegisterReply AccountExists = new RegisterReply
        {
            Result = ActionCode.AccountExists,
            Message = "Произошла ошибка: пользователь уже существует."
        };

        public static readonly RegisterReply SuccessfulRegister = new RegisterReply
        {
            Result = ActionCode.Successful,
            Message = "Регистрация завершена успешно."
        };
    }
}
