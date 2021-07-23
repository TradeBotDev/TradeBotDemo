using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace Account.AccountMessages
{
    public static class AddExchangeAccessReplies
    {
        public static readonly AddExchangeAccessReply SuccessfulAddition = new AddExchangeAccessReply
        {
            Result = ActionCode.Successful,
            Message = "Добавление биржи в аккаунт пользователя завершено."
        };

        public static readonly AddExchangeAccessReply FailedAddition = new AddExchangeAccessReply
        {
            Result = ActionCode.AccountNotFound,
            Message = "Произошла ошибка добавления биржи: пользователь не существует."
        };
    }
}
