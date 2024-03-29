﻿using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Instruments;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Wallets;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.DataTransfering.Bitmex.Rest.Responses;
using TradeMarket.Model.Publishers;
using TradeMarket.Model.UserContexts;
using TradeMarket.Model.UserContexts.Builders;
using Margin = Bitmex.Client.Websocket.Responses.Margins.Margin;
using Order = Bitmex.Client.Websocket.Responses.Orders.Order;

namespace TradeMarket.Model.TradeMarkets
{
    public abstract class TradeMarket
    {

        public string Name { get; internal set; }

        public IPublisherFactory PublisherFactory { get; internal set; }

        public RestfulClient CommonRestClient { get; set; }


        public TradeMarket() { }

        public TradeMarket(string name,IPublisherFactory publisherFactory)
        {
            Name = name;
            PublisherFactory = publisherFactory;
        }

        public abstract Task<Context> BuildContextAsync(ContextBuilder builder,CancellationToken token, ILogger logger);


        #region Common Publishers
        //IContext = CommonContext
        public IDictionary<Context, IPublisher<BookLevel>> Book25Publisher { get; internal set; } = new ConcurrentDictionary<Context, IPublisher<BookLevel>>();

        public IDictionary<Context, IPublisher<Instrument>> InstrumentPublisher { get; internal set; } = new ConcurrentDictionary<Context, IPublisher<Instrument>>();

        #endregion


        public async Task<List<T>> SubscribeToAsync<T>(
            IDictionary<Context,IPublisher<T>> publisher, 
            EventHandler<IPublisher<T>.ChangedEventArgs> handler, 
            Context context, 
            IPublisherFactory.Create<T> create,
            CancellationToken token, 
            ILogger logger)
        {
            var log = logger;
            log.Information("Subscribing");
            if(publisher.ContainsKey(context) == false)
            {
                log.Information("Creatung publisher");
                publisher[context] = await CreatePublisherAsync( context, token, create);
            }
            return await SubscribeToAsync(publisher[context], handler, log);
        }

        public IDictionary<Context, IPublisher<Wallet>> WalletPublishers { get; internal set; } = new ConcurrentDictionary<Context, IPublisher<Wallet>>();
        public IDictionary<Context, IPublisher<Position>> PositionPublisher { get; internal set; } = new ConcurrentDictionary<Context, IPublisher<Position>>();
        public IDictionary<Context, IPublisher<Order>> OrderPublisher { get; internal set; } = new ConcurrentDictionary<Context, IPublisher<Order>>();
        public IDictionary<Context, IPublisher<Margin>> MarginPublisher { get; internal set; } = new ConcurrentDictionary<Context, IPublisher<Margin>>();
        public IDictionary<Context, IPublisher<bool>> AuthenticationPublisher { get; internal set; } = new ConcurrentDictionary<Context, IPublisher<bool>>();




        public async Task<IPublisher<T>> CreatePublisherAsync<T>(Context context, CancellationToken token, IPublisherFactory.Create<T> create)
        {
            return await Task.Run(() =>
            {
                var publisher = create(context, token);
                //Log.Information("Created {@Publisher} for {@Context}", publisher, context);
                return publisher;
            });
        }
        public async Task<List<T>> SubscribeToAsync<T>(IPublisher<T> publisher, EventHandler<IPublisher<T>.ChangedEventArgs> handler, ILogger logger)
        {
            var log = logger;
            if (publisher is null)
            {
                throw new ArgumentException($"Publisher {nameof(publisher)} was null.");
            }
            if (publisher.IsWorking == false)
            {
                await publisher.Start(log);
            }
            publisher.Changed += handler;
            log.Information("Added Handler for Publisher", handler, publisher);
            return publisher.Cache;
        }
        public async Task UnsubscribeFromAsync<T>(IPublisher<T> publisher, EventHandler<IPublisher<T>.ChangedEventArgs> handler, ILogger logger)
        {
            await Task.Run(() =>
            {
                var log = logger; 
                if (publisher is not null)
                {
                    log.Information("Removing Handler");
                    publisher.Changed -= handler;
                }
           });
        }
        public async Task UnsubscribeFromAsync<T>(IDictionary<Context, IPublisher<T>> publisher,Context context , EventHandler<IPublisher<T>.ChangedEventArgs> handler, ILogger logger)
        {
            await Task.Run(async () =>
            {
                var contextPublisher = publisher.ContainsKey(context) ? publisher[context] : null;
                var log = logger
                    .ForContext("Method", nameof(UnsubscribeFromAsync))
                    .ForContext("@PublisherDictionary", publisher)
                    .ForContext("@Publisher",contextPublisher);

                Log.Information("Unsubscribing", publisher, contextPublisher);
                await UnsubscribeFromAsync(contextPublisher, handler,log);
                if(contextPublisher.SubscribersCount == 0)
                {
                    Log.Information("Stoping", publisher, contextPublisher);
                    await contextPublisher.Stop(log);
                    Log.Information("Removing", publisher, contextPublisher);
                    publisher.Remove(context);
                }
            });
        }

      

        #region Commands

        public abstract Task<bool> AutheticateUserAsync(Context context, CancellationToken token, ILogger logger);

        public abstract Task<ResfulResponse<Order>> PlaceOrderAsync(double quontity, double price, Context context, CancellationToken token, ILogger logger);

        public abstract Task<ResfulResponse<Order[]>> DeleteOrderAsync(string id, Context context, CancellationToken token, ILogger logger);

        public abstract Task<ResfulResponse<Order>> AmmendOrderAsync(string id,double? price,long? Quantity,long? LeavesQuantity, Context context, CancellationToken token, ILogger logger);
        
        #endregion





    }
}
