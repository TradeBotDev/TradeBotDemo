using System.Collections.Generic;
using System.Linq;
using TradeBot.Common.v1;

namespace Former
{
    public static class Clients
    {
        //Класс для хранения контекстов клиентов
        private static readonly List<UserContext> Contexts = new();

        //Получает контекст пользователя по его sessionId, trademarket name, slot name если он уже существует или создаёт новый
        public static UserContext GetUserContext(string sessionId, string tradeMarket, string slot, Config config = null)
        {
            var result = Contexts.FirstOrDefault(el => el.SessionId == sessionId && el.TradeMarket == tradeMarket && el.Slot == slot);
            if (result is not null) return result;
            result = new UserContext(sessionId, tradeMarket, slot, config);
            Contexts.Add(result);
            return result;
        }
    }
}
