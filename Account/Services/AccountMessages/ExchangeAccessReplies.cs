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

        public static ExchangesBySessionReply SuccessfulGettingExchangesInfo(IQueryable<Models.ExchangeAccess> exchangesFromAccount)
        {
            ExchangesBySessionReply reply = new ExchangesBySessionReply
            {
                Message = "Получение информации о биржах завершено успешно.",
                Result = ActionCode.Successful,
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

        public static readonly ExchangesBySessionReply ExchangesNotFound = new ExchangesBySessionReply
        {
            Message = "Ошибка при получении бирж: данные не найдены.",
            Result = ActionCode.ExchangeNotFound
        };
    }
}
