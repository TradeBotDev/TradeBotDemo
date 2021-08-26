using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Messages;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Websockets;
using Serilog;
using TradeMarket.Model.Publishers;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public abstract class BitmexPublisher<TResponse,TRequest,TModel> : IPublisher<TModel>
        where TResponse : MessageBase
        where TRequest : RequestBase
    {
        /// <summary>
        /// Действие которое выполняет при поступлении новых данных с биржи
        /// </summary>
        //public для тестов ((
        public readonly Action<TResponse, EventHandler<IPublisher<TModel>.ChangedEventArgs>,ILogger> _onNext;

        public event EventHandler<IPublisher<TModel>.ChangedEventArgs> Changed;


        internal CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        protected readonly BitmexWebsocketClient _client;

        protected List<TModel> _cache;

        public object locker = new();


        public List<TModel> Cache { get{ lock (locker) { return new(_cache); } }  set => _cache = value; }

        public bool IsWorking { get; protected set; } = false;

        public int SubscribersCount => Changed is not null ? Changed.GetInvocationList().Length : 0;

        public BitmexPublisher(BitmexWebsocketClient client,Action<TResponse, EventHandler<IPublisher<TModel>.ChangedEventArgs>,ILogger> action)
        {
            _client = client;
            _onNext = action;
            Cache = new();
        }

        protected void ClearCahce(ILogger logger)
        {
            var log = logger.ForContext("Method", nameof(ClearCahce));
            lock (locker)
            {
                _cache.Clear();
            }
        }
        

        public abstract void AddModelToCache(TResponse response);

        protected RequestBase CreateUnsubsscribeReqiest(SubscribeRequestBase request)
        {
            request.IsUnsubscribe = true;
            return request;
        }


        protected async Task UnSubscribeAsync<T>(T request, ILogger logger) where T : SubscribeRequestBase 
        {
            var log = logger.ForContext<BitmexPublisher<TResponse, TRequest, TModel>>().ForContext("Method", nameof(UnSubscribeAsync));
            log.Information("Sending unsubscribe request");
            await Task.Run(() => _client.Send(CreateUnsubsscribeReqiest(request)));
            //Log.Information("Successfully unsubscribed from topic {@Topic}", request.Topic);
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            IsWorking = false;
        }
        protected async Task SubscribeAsync(TRequest request, IObservable<TResponse> stream, CancellationToken token, ILogger logger)
        {
            //TODO убрать отсюда токен
            var log = logger.ForContext<BitmexPublisher<TResponse, TRequest, TModel>>().ForContext("Method",nameof(SubscribeAsync));

            void responseAction(TResponse response)
            {
                log.Information("Adding Response to Cache", response);
                AddModelToCache(response);
                log.Information("Invoking Event with response ", response);
                _onNext.Invoke(response, Changed,log);
            }

            await Task.Run(() =>
           {
               try
               {
                   IsWorking = true;
                   stream.Subscribe(responseAction, cancellationTokenSource.Token);
                   log.Information("Subscribed");

                   if (request is not null) _client.Send(request);
               }
               catch
               {
                   log.Error("Exception Catched");
                   throw;
               }
           });
        }

        public abstract Task Start(ILogger logger);

        public async virtual Task Stop(ILogger logger)
        {
            await Task.Run(() =>
            {
                cancellationTokenSource.Cancel();
            });
        }
    }
}
