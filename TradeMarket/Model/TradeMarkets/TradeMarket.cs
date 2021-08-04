using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Instruments;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Wallets;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;
using TradeMarket.DataTransfering;
using TradeMarket.DataTransfering.Bitmex;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.Model;
using TradeMarket.Model.Publishers;
using TradeMarket.Model.UserContexts;
using Margin = Bitmex.Client.Websocket.Responses.Margins.Margin;
using Order = Bitmex.Client.Websocket.Responses.Orders.Order;

namespace TradeMarket.Model.TradeMarkets
{
    public abstract class TradeMarket
    {
        public IConnectionMultiplexer Multiplexer { get; internal set; }

        public string Name { get; internal set; }

        public IPublisherFactory PublisherFactory { get; internal set; }

        public TradeMarket() { }

        public TradeMarket(string name,IPublisherFactory publisherFactory)
        {
            Name = name;
            PublisherFactory = publisherFactory;
        }

        #region Common Clients
        public BitmexWebsocketClient CommonWSClient { get; internal set; }
        public BitmexRestfulClient CommonRestClient { get; internal set; }
        #endregion

        #region Common Publishers
        public IPublisher<BookLevel> Book25Publisher { get; internal set; }

        public IPublisher<Instrument> InstrumentPublisher { get; internal set; }

        #endregion

        #region Users Publishers
        //TODO мб перенести эти штуки внутрь юзерконтекста
        public Dictionary<UserContext, IPublisher<Wallet>> WalletPublishers { get; internal set; } = new Dictionary<UserContext, IPublisher<Wallet>>();
        public Dictionary<UserContext, IPublisher<Position>> PositionPublisher { get; internal set; } = new Dictionary<UserContext, IPublisher<Position>>();
        public Dictionary<UserContext, IPublisher<Order>> OrderPublisher { get; internal set; } = new Dictionary<UserContext, IPublisher<Order>>();
        public Dictionary<UserContext, IPublisher<Margin>> MarginPublisher { get; internal set; } = new Dictionary<UserContext, IPublisher<Margin>>();
        public Dictionary<UserContext, IPublisher<bool>> AuthenticationPublisher { get; internal set; } = new Dictionary<UserContext, IPublisher<bool>>();
        #endregion

        #region Subscribe Methods
        
        public abstract void SubscribeToInstruments(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler, UserContext context);
        public abstract void SubscribeToUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler, UserContext context);
        public abstract void SubscribeToUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler, UserContext context);
        public abstract void SubscribeToBook25(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler, UserContext context);
        public abstract void SubscribeToUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler, UserContext context);
        public abstract void SubscribeToBalance(EventHandler<IPublisher<Wallet>.ChangedEventArgs> handler, UserContext context);

        #endregion

        #region Commands

        public abstract Task<DefaultResponse> AutheticateUser(UserContext context);

        public abstract Task<PlaceOrderResponse> PlaceOrder(double quontity, double price,UserContext context);

        public abstract Task<DefaultResponse> DeleteOrder(string id, UserContext context);

        public abstract Task<DefaultResponse> AmmendOrder(string id,double? price,long? Quantity,long? LeavesQuantity, UserContext context);
        
        #endregion





    }
}
