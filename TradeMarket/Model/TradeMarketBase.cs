using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.Model;

namespace TradeMarket.Model
{
    public abstract class TradeMarketBase
    {

        public abstract Task<bool> PlaceOrder(double quontity, double price);

        public abstract Task<bool> CloseOrder(string id);

        public abstract Task<bool> AutheticateUser(string api, string secret);

        public abstract event EventHandler<FullOrder> Book25Updated;

        public abstract event EventHandler<FullOrder> BookUpdated;

        public abstract event EventHandler<FullOrder> UserOrdersUpdated;

        public abstract event EventHandler<Balance> BalanceUpdated;

        //TODO  нужен общий тип
        public event EventHandler<string> ErrorUpdated;

        //TODO нужен общий тип
        public event EventHandler<string> LogsUpdated;

    }
}
