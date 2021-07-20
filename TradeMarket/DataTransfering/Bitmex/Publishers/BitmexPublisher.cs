using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Websockets;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public abstract class BitmexPublisher<TResponse,TRequest,TModel> : IPublisher<TModel>
        where TResponse : ResponseBase<TModel>
        where TRequest : RequestBase
    {

        private readonly Action<TResponse, EventHandler<IPublisher<TModel>.ChangedEventArgs>> _invokeActionOnNext;

        private readonly BitmexWebsocketClient _client;

        public event EventHandler<IPublisher<TModel>.ChangedEventArgs> Changed;

        public BitmexPublisher(Action<TResponse, EventHandler<IPublisher<TModel>.ChangedEventArgs>> action)
        {
            _invokeActionOnNext = action ?? throw new ArgumentNullException(nameof(action));
        }


        public async Task SubscribeAsync(TRequest request, IObservable<TResponse> stream, CancellationToken token)
        {

            _client.Send(request);
            //TODO это просто жесть c действием. чувствую что нужно как-то по другому
            stream.Subscribe(response => _invokeActionOnNext.Invoke(response, Changed));
            //TODO строчки ниже должна жить в классе биржи
            //await communicator.Start();
            //exitEvent.WaitOne(TimeSpan.FromSeconds(30));

            await AwaitCancellation(token);
        }
       
        private static Task AwaitCancellation(CancellationToken token)
        {
            var completion = new TaskCompletionSource<object>();
            token.Register(() => completion.SetResult(null));
            return completion.Task;
        }

        public void subscribeToEvent(object sender,TModel changed)
        {
            Changed?.Invoke(sender, new(changed));
        }
    }
}
