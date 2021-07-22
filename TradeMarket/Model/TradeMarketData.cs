using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex;

namespace TradeMarket.Model
{
    public static class TradeMarketData
    {
        internal static IDictionary<string, TradeMarket> ExistingTradeMarkets = new Dictionary<string, TradeMarket>(new List<KeyValuePair<string, TradeMarket>>{
            new KeyValuePair<string, TradeMarket>("Bitmex",new BitmexTradeMarket("Bitmex"))
        });


        public static TradeMarket GetTradeMarket(string name)
        {
            if (!ExistingTradeMarkets.ContainsKey(name))
            {
                throw new ArgumentException($"{name} hasn't been implemented yet");
            }
            return ExistingTradeMarkets[name];
        }
    }
}
