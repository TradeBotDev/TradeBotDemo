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

        public event EventHandler<FullOrder> Book25Updated;

        public event EventHandler<FullOrder> BookUpdated;

        public event EventHandler<FullOrder> UserOrdersUpdated;

        public event EventHandler<Balance> BalanceUpdated;

        //TODO  нужен общий тип
        public event EventHandler<string> ErrorUpdated;

        //TODO нужен общий тип
        public event EventHandler<string> LogsUpdated;

    }
}
