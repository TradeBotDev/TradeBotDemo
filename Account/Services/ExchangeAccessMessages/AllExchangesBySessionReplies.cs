using Serilog;
using System.Linq;
using TradeBot.Account.AccountService.v1;

namespace Account.AccountMessages
{
    public static class AllExchangesBySessionReplies
    {
        public static AllExchangesBySessionReply SuccessfulGetting(IQueryable<Models.ExchangeAccess> exchangesFromAccount)
        {
            const string Message = "Получение информации о биржах завершено успешно.";
            Log.Information(Message);

            AllExchangesBySessionReply reply = new AllExchangesBySessionReply
            {
                Result = ActionCode.Successful,
                Message = Message
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

        public static AllExchangesBySessionReply AccountNotFound()
        {
            const string Message = "Произошла ошибка получение данных бирж: пользователь не существует.";
            Log.Information(Message);

            return new AllExchangesBySessionReply
            {
                Result = ActionCode.AccountNotFound,
                Message = Message
            };
        }

        public static AllExchangesBySessionReply ExchangesNotFound()
        {
            const string Message = "Ошибка при получении бирж: данные не найдены.";
            Log.Information(Message);

            return new AllExchangesBySessionReply
            {
                Result = ActionCode.ExchangeNotFound,
                Message = Message
            };
        }
    }
}
