using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering.Bitmex
{
    public class BitmexTradeMarket : Model.TradeMarketBase
    {
        public override Task<bool> AutheticateUser(string api, string secret)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> CloseOrder(string id)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> PlaceOrder(double quontity, double price)
        {
            throw new NotImplementedException();
        }
    }
}
