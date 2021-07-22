using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace Account.AccountMessages
{
    public static class ExchangesBySessionReplies
    {
        public static ExchangesBySessionReply SuccessfulGetting(IQueryable<Models.ExchangeAccess> exchangesFromAccount)
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

        public static readonly ExchangesBySessionReply AccountNotFound = new ExchangesBySessionReply
        {
            Result = ActionCode.AccountNotFound,
            Message = "Произошла ошибка получение данных бирж: пользователь не существует."
        };

        public static readonly ExchangesBySessionReply ExchangesNotFound = new ExchangesBySessionReply
        {
            Result = ActionCode.ExchangeNotFound,
            Message = "Ошибка при получении бирж: данные не найдены."
        };
    }
}
