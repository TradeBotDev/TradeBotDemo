using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex;

namespace TradeMarket.Model
{
    public static class UserContextData
    {
        internal static List<BitmexUserContext> RegisteredUsers = new List<BitmexUserContext>();

        public static BitmexUserContext GetUserContext(string sessionId, string slotName)
        {
            if (RegisteredUsers.FirstOrDefault(el => el.IsEquevalentTo(sessionId, slotName, "Bitmex")) is null)
            {
                RegisterUser(sessionId, slotName, "Bitmex");
            }
            return RegisteredUsers.First(el => el.IsEquevalentTo(sessionId, slotName, "Bitmex"));
        }

        public static void RegisterUser(string sessionId, string slotName, string tradeMarketName)
        {
            BitmexUserContext user = new BitmexUserContext(sessionId, slotName, TradeMarketData.GetTradeMarket(tradeMarketName));

            RegisteredUsers.Add(user);
        }
    }
}
