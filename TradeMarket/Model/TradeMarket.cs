using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;
using TradeMarket.DataTransfering.Bitmex;
using TradeMarket.Model;

namespace TradeMarket.Model
{
    public abstract class TradeMarket
    {
        #region Dynamic Part
        public string Name { get; internal set; }

        public abstract Task<PlaceOrderResponse> PlaceOrder(double quontity, double price,UserContext context);

        public abstract Task<DefaultResponse> DeleteOrder(string id, UserContext context);

        public abstract Task<DefaultResponse> AmmendOrder(string id,double? price,long? Quantity,long? LeavesQuantity, UserContext context);


        public abstract Task<DefaultResponse> AutheticateUser(string api, string secret,UserContext context);

        public abstract void SubscribeToBook25(EventHandler<FullOrder> handler, UserContext context);
        
        public abstract void SubscribeToBook(EventHandler<FullOrder> handler, UserContext context);

        public abstract void SubscribeToUserOrders(EventHandler<FullOrder> handler, UserContext context);

        public abstract void SubscribeToBalance(EventHandler<Balance> handler, UserContext context);


        public abstract event EventHandler<FullOrder> Book25Update;
        public abstract event EventHandler<FullOrder> BookUpdate;
        public abstract event EventHandler<FullOrder> UserOrdersUpdate;
        public abstract event EventHandler<Balance> BalanceUpdate;
        #endregion

        #region Static Part

        private static IDictionary<string, TradeMarket> _tradeMarkets = new Dictionary<string, TradeMarket>(new List<KeyValuePair<string, TradeMarket>>
        {
            new KeyValuePair<string, TradeMarket>("bitmex",new BitmexTradeMarket("bitmex"))
        });

        public static TradeMarket GetTradeMarket(string name)
        {
            if (_tradeMarkets.ContainsKey(name))
            {
                return _tradeMarkets[name];
            }
            throw new ArgumentException($"{name} hasn't been implemented yet");
        }
        #endregion
    }
}
