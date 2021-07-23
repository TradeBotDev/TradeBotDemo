using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace Account.AccountMessages
{
    public static class LoginReplies
    {
        public static LoginReply SuccessfulLogin(string sessionId)
        {
            return new LoginReply
            {
                SessionId = sessionId,
                Result = ActionCode.Successful,
                Message = "Произведен вход в аккаунт."
            };
        }

        public static LoginReply AlreadySignedIn(string sessionId)
        {
            return new LoginReply
            {
                SessionId = sessionId,
                Result = ActionCode.Successful,
                Message = "Вы уже вошли в аккаунт."
            };
        }

        public static readonly LoginReply AccountNotFound = new LoginReply
        {
            SessionId = "Отсутствует",
            Result = ActionCode.AccountNotFound,
            Message = "Ошибка при входе: пользователь не найден."
        };
    }
}
