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

        private Action<TResponse, EventHandler<IPublisher<TModel>.ChangedEventArgs>> _invokeActionOnNext;

        public event EventHandler<IPublisher<TModel>.ChangedEventArgs> Changed;

        public BitmexPublisher(Action<TResponse, EventHandler<IPublisher<TModel>.ChangedEventArgs>> action)
        {
            _invokeActionOnNext = action ?? throw new ArgumentNullException(nameof(action));
        }


        public async Task SubscribeAsync(TRequest request,IObservable<TResponse> stream, CancellationToken token)
        {
            var exitEvent = new ManualResetEvent(false);

            var url = BitmexValues.ApiWebsocketUrl;

            using (var communicator = new BitmexWebsocketCommunicator(url))
            {
                using (var client = new BitmexWebsocketClient(communicator))
                {
                    client.Send(request);
                    //TODO это просто жесть c действием. чувствую что нужно как-то по другому
                    stream.Subscribe(response => _invokeActionOnNext.Invoke(response,Changed));
                    await communicator.Start();
                    await AwaitCancellation(token);
                    //exitEvent.WaitOne(TimeSpan.FromSeconds(30));
                }
            }
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
