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

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public abstract class BitmexPublisher<TResponse,TRequest,TModel> : IPublisher<TModel>
        where TResponse : MessageBase
        where TRequest : RequestBase
    {

        private readonly Action<TResponse, EventHandler<IPublisher<TModel>.ChangedEventArgs>> _invokeActionOnNext;

        private readonly BitmexWebsocketClient _client;

        public event EventHandler<IPublisher<TModel>.ChangedEventArgs> Changed;

        public BitmexPublisher(BitmexWebsocketClient client,Action<TResponse, EventHandler<IPublisher<TModel>.ChangedEventArgs>> action)
        {
            _client = client;
            _invokeActionOnNext = action;// ?? throw new ArgumentNullException(nameof(action));
        }


        private void responseAction(TResponse response)
        {
            _invokeActionOnNext.Invoke(response, Changed);
        }

        internal async Task SubscribeAsync(TRequest request, IObservable<TResponse> stream, CancellationToken token)
        {
            _client.Send(request);
            stream.Subscribe(responseAction);
        }
       
        private static Task AwaitCancellation(CancellationToken token)
        {
            var completion = new TaskCompletionSource<object>();
            token.Register(() => completion.SetResult(null));
            return completion.Task;
        }

    }
}
