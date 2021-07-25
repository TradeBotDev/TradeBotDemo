using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Former
{
    public static class UserContextFactory
    {
        private static TradeMarketClient _tradeMarketClient = TradeMarketClient.GetInstance();
        
        private static Former _former = new Former();

        private static readonly List<UserContext> contexts = new List<UserContext>();

        public static UserContext GetUserContext(string sessionId, string trademarket, string slot)
        {
            UserContext result = contexts.FirstOrDefault(el => el.sessionId == sessionId && el.trademarket == trademarket && el.slot == slot);
            if (result is null)
            {
                result = new UserContext(null,null, sessionId, trademarket, slot);
                contexts.Add(result);
            }
            return result;
        }

       /* public static void AddUserContext(string sessionId, string trademarket, string slot, Config configuration) 
        {
            contexts.Add(new UserContext { sessionId = sessionId, trademarket = trademarket, slot= slot, configuration = configuration });
        }
        public static UserContext GetUserContextById(string key) 
        {
            return contexts.FirstOrDefault(x => x.sessionId == key);
        }
        public static UserContext GetUserContextByTradeMarket(string key)
        {
            return contexts.FirstOrDefault(x => x.trademarket == key);
        }
        public static UserContext GetUserContextBySlot(string key)
        {
            return contexts.FirstOrDefault(x => x.slot == key);
        }*/
        public static bool Contains(string key)
        {
            if (contexts.FindIndex(x => x.sessionId == key) != -1) return true;
            else return false;
        }

    }
}
