using System.Collections.Generic;
using System.Linq;
using TradeBot.Common.v1;

namespace Former.Model
{
    public static class Contexts
    {
        //Класс для хранения контекстов клиентов
        private static readonly List<UserContext> userContexts = new();

        //Получает контекст пользователя по его sessionId, trademarket name, slot name если он уже существует или создаёт новый
        public static UserContext GetUserContext(string sessionId, string tradeMarket, string slot, Config config = null)
        {
            var result = userContexts.FirstOrDefault(el => el.SessionId == sessionId && el.TradeMarket == tradeMarket && el.Slot == slot);
            if (result is not null) return result;
            result = new UserContext(sessionId, tradeMarket, slot, config);
            userContexts.Add(result);
            return result;
        }
    }
}
