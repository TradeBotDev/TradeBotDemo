using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Former
{
    public static class UserContextFactory
    {
        private static readonly List<UserContext> contexts = new List<UserContext>();

        public static UserContext GetUserContext(string sessionId, string trademarket, string slot)
        {
            UserContext result = contexts.FirstOrDefault(el => el.sessionId == sessionId && el.trademarket == trademarket && el.slot == slot);
            if (result is null)
            {
                result = new UserContext(sessionId, trademarket, slot);
                contexts.Add(result);
            }
            return result;
        }
    }
}
