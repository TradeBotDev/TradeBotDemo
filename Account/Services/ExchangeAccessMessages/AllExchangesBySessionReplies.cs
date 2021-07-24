using System.Linq;
using TradeBot.Account.AccountService.v1;

namespace Account.AccountMessages
{
    public static class AllExchangesBySessionReplies
    {
        public static AllExchangesBySessionReply SuccessfulGetting(IQueryable<Models.ExchangeAccess> exchangesFromAccount)
        {
            AllExchangesBySessionReply reply = new AllExchangesBySessionReply
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

        public static readonly AllExchangesBySessionReply AccountNotFound = new AllExchangesBySessionReply
        {
            Result = ActionCode.AccountNotFound,
            Message = "Произошла ошибка получение данных бирж: пользователь не существует."
        };

        public static readonly AllExchangesBySessionReply ExchangesNotFound = new AllExchangesBySessionReply
        {
            Result = ActionCode.ExchangeNotFound,
            Message = "Ошибка при получении бирж: данные не найдены."
        };
    }
}
