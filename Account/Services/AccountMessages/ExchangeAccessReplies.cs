using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace Account.AccountMessages
{
    public static class ExchangeAccessReplies
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

        public static ExchangesBySessionReply SuccessfulGettingExchangesInfo(IQueryable<Models.ExchangeAccess> exchangesFromAccount)
        {
            ExchangesBySessionReply reply = new ExchangesBySessionReply
            {
                Result = ActionCode.Successful,
                Message = "Получение информации о биржах завершено успешно."
            };

            foreach (Models.ExchangeAccess exchange in exchangesFromAccount)
            {
                reply.Exchanges.Add(new ExchangeAccessInfo
                {
                    ExchangeAccessId = exchange.ExchangeAccessId,
                    Code = exchange.Code,
                    Name = exchange.Name,
                    Token = exchange.Token,
                    Secret = exchange.Secret
                });
            }
            return reply;
        }

        public static readonly ExchangesBySessionReply AccountWithExchangesNotFound = new ExchangesBySessionReply
        {
            Result = ActionCode.AccountNotFound,
            Message = "Произошла ошибка получение данных бирж: пользователь не существует."
        };

        public static readonly ExchangesBySessionReply ExchangesNotFound = new ExchangesBySessionReply
        {
            Result = ActionCode.ExchangeNotFound,
            Message = "Ошибка при получении бирж: данные не найдены."
        };

        public static readonly DeleteExchangeAccessReply SuccessfulDeletingExchangeAccess = new DeleteExchangeAccessReply
        {
            Result = ActionCode.Successful,
            Message = "Данные биржи для данного пользователя успешно удалены."
        };

        public static readonly DeleteExchangeAccessReply ExchangeAccessNotFound = new DeleteExchangeAccessReply
        {
            Result = ActionCode.ExchangeNotFound,
            Message = "Данные биржи не найдены."
        };
    }
}
