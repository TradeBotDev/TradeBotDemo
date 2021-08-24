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
        public readonly Action<TResponse, EventHandler<IPublisher<TModel>.ChangedEventArgs>> _onNext;

        public event EventHandler<IPublisher<TModel>.ChangedEventArgs> Changed;


        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        protected readonly BitmexWebsocketClient _client;

        protected List<TModel> _cache;

        public object locker = new();


        public List<TModel> Cache { get{ lock (locker) { return new(_cache); } }  set => _cache = value; }

        public bool IsWorking { get; internal set; } = false;

        public int SubscribersCount => Changed is not null ? Changed.GetInvocationList().Length : 0;

        public BitmexPublisher(BitmexWebsocketClient client,Action<TResponse, EventHandler<IPublisher<TModel>.ChangedEventArgs>> action)
        {
            _client = client;
            _onNext = action;
            Cache = new();
        }

        protected void ClearCahce()
        {
            lock (locker)
            {
                _cache.Clear();
            }
        }
        private void responseAction(TResponse response)
        {
            Log.Information("Adding Response {@Response} to Cache", response);
            AddModelToCache(response);
            Log.Information("Invoking Event with response {@Response}", response);
            _onNext.Invoke(response, Changed);
        }

        public abstract void AddModelToCache(TResponse response);

        protected RequestBase CreateUnsubsscribeReqiest(SubscribeRequestBase request)
        {
            request.IsUnsubscribe = true;
            return request;
        }


        protected async Task UnSubscribeAsync<T>(T request) where T : SubscribeRequestBase 
        {
            Log.Information("Unsubscribing from topic {@Topic}", request.Topic);
            await Task.Run(() => _client.Send(CreateUnsubsscribeReqiest(request)));
            Log.Information("Successfully unsubscribed from topic {@Topic}", request.Topic);
            IsWorking = false;
        }
        protected async Task SubscribeAsync(TRequest request, IObservable<TResponse> stream, CancellationToken token)
        {
           //TODO убрать отсюда токен
           await Task.Run(() =>
           {
               if (request is not null)
               {
                   Log.Information("Subscribing For topic {@Topic}", request is SubscribeRequestBase ? (request as SubscribeRequestBase).Topic : request.OperationString);
               }
               IsWorking = true;
               stream.Subscribe(responseAction, cancellationTokenSource.Token);

               if (request is not null) _client.Send(request);
           });
        }

        public abstract Task Start();

        public abstract Task Stop();
    }
}
