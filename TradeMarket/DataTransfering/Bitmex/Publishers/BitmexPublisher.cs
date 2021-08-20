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

        public bool IsWorking { get; private set; } = false;

        public BitmexPublisher(BitmexWebsocketClient client,Action<TResponse, EventHandler<IPublisher<TModel>.ChangedEventArgs>> action)
        {
            _client = client;
            _onNext = action;
            Cache = new();
        }

        private void ClearCahce()
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

        internal async Task SubscribeAsync(TRequest request, IObservable<TResponse> stream, CancellationToken token)
        {
           token.Register(() => {
               //1 потому что сначала прилетает токен а потом идет отписка от ивента
               Log.Warning("Remaining listeners {@ListenersCount}", Changed?.GetInvocationList().Length);
                if(Changed?.GetInvocationList().Length == 1)
               {
                   cancellationTokenSource.Cancel();
                   if (request is SubscribeRequestBase)
                   {
                       _client.Send(CreateUnsubsscribeReqiest(request as SubscribeRequestBase));
                       ClearCahce();
                   }
                   (request as SubscribeRequestBase).IsUnsubscribe = false;
                   IsWorking = false;
                   if (request is not null)
                   {
                       Log.Information("Subscriber Stoped Working for topic {@Topic}", request is SubscribeRequestBase ? (request as SubscribeRequestBase).Topic : request.OperationString);
                   }
               }
           });
           await Task.Run(() =>
           {
               if (request is not null)
               {
                   Log.Information("Subscribed For topic {@Topic}", request is SubscribeRequestBase ? (request as SubscribeRequestBase).Topic : request.OperationString);
               }
               IsWorking = true;
               //тут не нужно ловить OperationCanceledException. BitmexWebsocketClient все разруливает сам
               //TODO тестирование 
               stream.Subscribe(responseAction, cancellationTokenSource.Token);

               if (request is not null) _client.Send(request);
           });
        }

        public abstract Task Start();

    }
}
