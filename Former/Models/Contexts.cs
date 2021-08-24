using System.Collections.Generic;
using System.Linq;
using Grpc.Core;
using Serilog;

namespace Former.Models
{
    public static class Contexts
    {
        //Класс для хранения контекстов клиентов
        private static readonly List<UserContext> UserContexts = new();
        private static readonly object locker = new ();

        //Получает контекст пользователя по его sessionId, trademarket name, slot name если он уже существует или создаёт новый
        public static UserContext GetUserContext(string sessionId, string tradeMarket, string slot)
        {
            lock (locker)
            {
                var result = UserContexts.FirstOrDefault(el => el.SessionId == sessionId && el.TradeMarket == tradeMarket && el.Slot == slot);
                if (result is not null) return result;
                result = new UserContext(sessionId, tradeMarket, slot);
                UserContexts.Add(result);
                Log.Information("Created new context {@sessionid}, {@trademarket}, {@slot}, number Of contexts {@NumberOfContexts}", sessionId, tradeMarket, slot, UserContexts.Count);
                return result;
            }
        }

        public static void ClearContexts()
        {
            UserContexts.Clear();
        }
    }
}
