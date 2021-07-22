using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeMarket.DataTransfering.Bitmex;
using TradeMarket.Model;

namespace TradeMarket.Model
{
    public abstract class TradeMarket
    {
        internal static IDictionary<string, TradeMarket> ExistingTradeMarkets = new Dictionary<string, TradeMarket>(new List<KeyValuePair<string, TradeMarket>>{
            new KeyValuePair<string, TradeMarket>("Bitmex",new BitmexTradeMarket())
        });

        internal static List<BitmexUserContext> RegisteredUsers = new List<BitmexUserContext>();
        
        public static TradeMarket GetTradeMarketByName(string name)
        {
            if (!ExistingTradeMarkets.ContainsKey(name))
            {
                throw new ArgumentException($"{name} hasn't been implemented yet");
            }
            return ExistingTradeMarkets[name];
        }

        public static BitmexUserContext GetUserContex(string sessionId,string slotName)
        {
            if(RegisteredUsers.FirstOrDefault(el => el.SessionId == sessionId && el.SlotName == slotName) is null)
            {
                RegisterUser(sessionId, slotName, "Bitmex");
            }
            return RegisteredUsers.First(el => el.SessionId == sessionId && el.SlotName == slotName);
        }

        public static void RegisterUser(string sessionId,string slotName,string tradeMarketName)
        {
            BitmexUserContext user = new BitmexUserContext(sessionId,slotName, GetTradeMarketByName(tradeMarketName));

            RegisteredUsers.Add(user);
        }


        public abstract Task<DefaultResponse> PlaceOrder(double quontity, double price,BitmexUserContext context);

        public abstract Task<DefaultResponse> CloseOrder(string id, BitmexUserContext context);

        public abstract Task<DefaultResponse> AutheticateUser(string api, string secret,BitmexUserContext context);

        public abstract void SubscribeToBook25(EventHandler<FullOrder> handler, BitmexUserContext context);
        
        public abstract void SubscribeToBook(EventHandler<FullOrder> handler, BitmexUserContext context);

        public abstract void SubscribeToUserOrders(EventHandler<FullOrder> handler, BitmexUserContext context);

        public abstract void SubscribeToBalance(EventHandler<Balance> handler, BitmexUserContext context);


        public abstract event EventHandler<FullOrder> Book25Update;
        public abstract event EventHandler<FullOrder> BookUpdate;
        public abstract event EventHandler<FullOrder> UserOrdersUpdate;
        public abstract event EventHandler<Balance> BalanceUpdate;

    }
    }
