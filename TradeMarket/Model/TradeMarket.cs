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
        public string Name { get; internal set; }

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
