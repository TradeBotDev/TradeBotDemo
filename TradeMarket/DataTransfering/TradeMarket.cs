using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TradeBot.Common.v1;

using TradeMarket.DataTransfering.Bitmex;
using TradeMarket.Model;

using Balance = TradeMarket.Model.Balance;

namespace TradeMarket.DataTransfering
{
    public abstract class TradeMarket
    {
        private static readonly IDictionary<string, TradeMarket> ExistingTradeMarkets = new Dictionary<string, TradeMarket>(new List<KeyValuePair<string, TradeMarket>>{
            new("Bitmex",new BitmexTradeMarket())
        });

        private static readonly List<BitmexUserContext> RegisteredUsers = new List<BitmexUserContext>();

        public static TradeMarket GetTradeMarketByName(string name)
        {
            if (!ExistingTradeMarkets.ContainsKey(name))
            {
                throw new ArgumentException($"{name} hasn't been implemented yet");
            }
            return ExistingTradeMarkets[name];
        }

        public static BitmexUserContext GetUserContextBySessionId(string sessionId)
        {
            return RegisteredUsers.First(el => el.SessionId == sessionId);
        }

        public static void RegisterUser(string sessionId, string tradeMarketName)
        {
            var user = new BitmexUserContext(sessionId)
            {
                TradeMarket = GetTradeMarketByName(tradeMarketName)
            };
            RegisteredUsers.Add(user);
        }


        public abstract Task<DefaultResponse> PlaceOrder(double quontity, double price, BitmexUserContext context);

        public abstract Task<DefaultResponse> CloseOrder(string id, BitmexUserContext context);

        public abstract Task<DefaultResponse> AutheticateUser(string api, string secret, BitmexUserContext context);

        public abstract void SubscribeToBook25(EventHandler<FullOrder> handler, BitmexUserContext context);

        public abstract void SubscribeToBook(EventHandler<FullOrder> handler, BitmexUserContext context);

        public abstract void SubscribeToUserOrders(EventHandler<FullOrder> handler, BitmexUserContext context);

        public abstract void SubscribeToBalance(EventHandler<Balance> handler, BitmexUserContext context);

    }
}
