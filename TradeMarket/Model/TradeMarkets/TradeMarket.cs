using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Instruments;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Wallets;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;
using TradeMarket.DataTransfering;
using TradeMarket.DataTransfering.Bitmex;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.DataTransfering.Bitmex.Rest.Responses;
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
        //IContext = CommonContext
        public IDictionary<IContext, IPublisher<BookLevel>> Book25Publisher { get; internal set; } = new ConcurrentDictionary<IContext, IPublisher<BookLevel>>();

        public IDictionary<IContext, IPublisher<Instrument>> InstrumentPublisher { get; internal set; } = new ConcurrentDictionary<IContext, IPublisher<Instrument>>();

        #endregion

        #region Users Publishers
        public async Task<IPublisher<T>> CreatePublisher<T>(BitmexWebsocketClient client, IContext context,CancellationToken token, IPublisherFactory.Create<T> create)
        {
            return await Task.Run(() =>
            {
                var publisher = create(client, context,token);
                return publisher;
            });
        }
        public async Task<List<T>> SubscribeTo<T>(BitmexWebsocketClient client,IPublisher<T> publisher,EventHandler<IPublisher<T>.ChangedEventArgs> handler,IContext context,CancellationToken token)
        {
            if(publisher is null)
            {
                throw new ArgumentException($"Publisher {nameof(publisher)} was null.");
            }
            if(publisher.IsWorking == false)
            {
                await publisher.Start();
            }
            Log.Information("Added Handler {@Handler} for Publisher {@PublisherName} ",handler,publisher);
            publisher.Changed += handler;
            return publisher.Cache;
        }

        public async Task<List<T>> SubscribeTo<T>(BitmexWebsocketClient client,IDictionary<IContext,IPublisher<T>> publisher, EventHandler<IPublisher<T>.ChangedEventArgs> handler, IContext context, IPublisherFactory.Create<T> create,CancellationToken token)
        {
            if(publisher.ContainsKey(context) == false)
            {
                publisher[context] = await CreatePublisher(client, context, token, create);
                await publisher[context].Start();
            }
            return await SubscribeTo(client, publisher[context],handler, context, token);
        }

        public IDictionary<IContext, IPublisher<Wallet>> WalletPublishers { get; internal set; } = new ConcurrentDictionary<IContext, IPublisher<Wallet>>();
        public IDictionary<IContext, IPublisher<Position>> PositionPublisher { get; internal set; } = new ConcurrentDictionary<IContext, IPublisher<Position>>();
        public IDictionary<IContext, IPublisher<Order>> OrderPublisher { get; internal set; } = new ConcurrentDictionary<IContext, IPublisher<Order>>();
        public IDictionary<IContext, IPublisher<Margin>> MarginPublisher { get; internal set; } = new ConcurrentDictionary<IContext, IPublisher<Margin>>();
        public IDictionary<IContext, IPublisher<bool>> AuthenticationPublisher { get; internal set; } = new ConcurrentDictionary<IContext, IPublisher<bool>>();
        #endregion

        #region Subscribe Methods
        #region Common Subscriptions
        public abstract Task<List<BookLevel>> SubscribeToBook25(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler, IContext context, CancellationToken token);
        public abstract Task<List<Instrument>> SubscribeToInstruments(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler, IContext context, CancellationToken token);
        #endregion
        #region User Subscriptions
        public abstract Task<List<Position>> SubscribeToUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler, UserContext context, CancellationToken token);
        public abstract Task<List<Margin>> SubscribeToUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler, UserContext context, CancellationToken token);
        public abstract Task<List<Order>> SubscribeToUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler, UserContext context, CancellationToken token);
        public abstract Task<List<Wallet>> SubscribeToBalance(EventHandler<IPublisher<Wallet>.ChangedEventArgs> handler, UserContext context, CancellationToken token);
        #endregion
        #endregion

        #region Unsubscribe Methods
        public async Task UnsubscribeFrom<T>(IPublisher<T> publisher, EventHandler<IPublisher<T>.ChangedEventArgs> handler)
        {
            await Task.Run(() =>
           {
               if (publisher is not null)
               {
                   publisher.Changed -= handler;
               }
           });
        }
        public async Task UnsubscribeFrom<T>(IDictionary<IContext, IPublisher<T>> publisher,IContext context , EventHandler<IPublisher<T>.ChangedEventArgs> handler)
        {
            await Task.Run(async () =>
            {
                var contextPublisher = publisher.ContainsKey(context) ? publisher[context] : null;
                await UnsubscribeFrom(contextPublisher, handler);
            });
        }

        #region Common Subscriptions
        public abstract Task UnSubscribeFromBook25(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler,IContext context);
        public abstract Task UnSubscribeFromInstruments(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler,IContext context);
        #endregion
        #region User Subscriptions
        public abstract Task UnSubscribeFromUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler,IContext context);
        public abstract Task UnSubscribeFromUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler, IContext context);
        public abstract Task UnSubscribeFromUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler, IContext context);
        public abstract Task UnSubscribeFromBalance(EventHandler<IPublisher<Wallet>.ChangedEventArgs> handler, IContext context);
        #endregion
        #endregion

        #region Commands

        public abstract Task<bool> AutheticateUser(UserContext context, CancellationToken token);

        public abstract Task<BitmexResfulResponse<Order>> PlaceOrder(double quontity, double price,IContext context, CancellationToken token);

        public abstract Task<BitmexResfulResponse<Order[]>> DeleteOrder(string id, IContext context, CancellationToken token);

        public abstract Task<BitmexResfulResponse<Order>> AmmendOrder(string id,double? price,long? Quantity,long? LeavesQuantity, IContext context, CancellationToken token);
        
        #endregion





    }
}
