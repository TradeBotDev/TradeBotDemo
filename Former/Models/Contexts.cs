using System.Collections.Generic;
using System.Linq;
using Former.Models;

namespace Former.Models
{
    public static class Contexts
    {
        //Класс для хранения контекстов клиентов
        private static readonly List<UserContext> UserContexts = new();

        //Получает контекст пользователя по его sessionId, trademarket name, slot name если он уже существует или создаёт новый
        public static UserContext GetUserContext(string sessionId, string tradeMarket, string slot)
        {
            var result = UserContexts.FirstOrDefault(el => el.SessionId == sessionId && el.TradeMarket == tradeMarket && el.Slot == slot);
            if (result is not null) return result;
            result = new UserContext(sessionId, tradeMarket, slot);
            UserContexts.Add(result);
            return result;
        }
    }
}
