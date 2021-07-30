using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;
using TradeMarket.DataTransfering;
using TradeMarket.DataTransfering.Bitmex;
using TradeMarket.Model;
using Margin = Bitmex.Client.Websocket.Responses.Margins.Margin;
using Order = Bitmex.Client.Websocket.Responses.Orders.Order;

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

        public abstract void SubscribeToBook25(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler, UserContext context);
        
        public abstract void SubscribeToBook(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler, UserContext context);

        public abstract void SubscribeToUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler, UserContext context);

        public abstract void SubscribeToBalance(EventHandler<Wallet> handler, UserContext context);

        public abstract void SubscribeToUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler, UserContext context);

        public abstract void SubscribeToUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler, UserContext context);



        public abstract event EventHandler<IPublisher<BookLevel>.ChangedEventArgs> Book25Update;
        public abstract event EventHandler<IPublisher<BookLevel>.ChangedEventArgs> BookUpdate;
        public abstract event EventHandler<IPublisher<Order>.ChangedEventArgs> UserOrdersUpdate;
        public abstract event EventHandler<Wallet> BalanceUpdate;
        public abstract event EventHandler<IPublisher<Margin>.ChangedEventArgs> MarginUpdate;
        public abstract event EventHandler<IPublisher<Position>.ChangedEventArgs> PositionUpdate;
        #endregion

        
    }
}
