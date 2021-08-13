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
        private readonly Action<TResponse, EventHandler<IPublisher<TModel>.ChangedEventArgs>> _onNext;
        
        public event EventHandler<IPublisher<TModel>.ChangedEventArgs> Changed;

        private readonly BitmexWebsocketClient _client;

        public BitmexPublisher(BitmexWebsocketClient client,Action<TResponse, EventHandler<IPublisher<TModel>.ChangedEventArgs>> action)
        {
            _client = client;
            _onNext = action;
        }


        private void responseAction(TResponse response)
        {
            _onNext.Invoke(response, Changed);
        }

        internal async Task SubscribeAsync(TRequest request, IObservable<TResponse> stream, CancellationToken token)
        {
           await Task.Run(() =>
           {
               //тут не нужно ловить OperationCanceledException. BitmexWebsocketClient все разруливает сам
               _client.Send(request);
               stream.Subscribe(responseAction, token);
           });
        }

        public abstract Task Start();
    }
}
