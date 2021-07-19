using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Websockets;

namespace TradeMarket.DataTransfering
{
    public class BitmexOrderSubscriber : ISubscriber<TradeMarket.Model.FullOrder>
    {
        private static BitmexOrderSubscriber _bitmexOrderSubscriber = null;

        public static BitmexOrderSubscriber GetInstance()
        {
            if(_bitmexOrderSubscriber == null)
            {
                _bitmexOrderSubscriber = new BitmexOrderSubscriber();
            }
            return _bitmexOrderSubscriber;
        }

        public event ISubscriber<TradeMarket.Model.FullOrder>.ChangedEventHandler Changed;

        public async Task SubscribeAsync(CancellationToken token)
        {
            var exitEvent = new ManualResetEvent(false);

            var url = BitmexValues.ApiWebsocketUrl;

            using (var communicator = new BitmexWebsocketCommunicator(url))
            {
                using (var client = new BitmexWebsocketClient(communicator))
                {
                    client.Send<BookSubscribeRequest>(new BookSubscribeRequest("XBTUSD"));
                    client.Streams.BookStream.Subscribe(response => { 
                        
                        foreach (var order in response.Data)
                        {
                            TradeBot.Common.v1.OrderSignature signature = new TradeBot.Common.v1.OrderSignature
                            {
                                Status = TradeBot.Common.v1.OrderStatus.Unspecified,
                                Type = order.Side == Bitmex.Client.Websocket.Responses.BitmexSide.Buy ? TradeBot.Common.v1.OrderType.Buy : TradeBot.Common.v1.OrderType.Sell
                            };
                            switch (response.Action)
                            {
                                case Bitmex.Client.Websocket.Responses.BitmexAction.Insert:  signature.Status = TradeBot.Common.v1.OrderStatus.Open;break;                           
                                case Bitmex.Client.Websocket.Responses.BitmexAction.Delete:  signature.Status = TradeBot.Common.v1.OrderStatus.Closed;break;
                                case Bitmex.Client.Websocket.Responses.BitmexAction.Partial: signature.Status = TradeBot.Common.v1.OrderStatus.Open; break;
                                case Bitmex.Client.Websocket.Responses.BitmexAction.Update: signature.Status = order.Size.HasValue? TradeBot.Common.v1.OrderStatus.Open : TradeBot.Common.v1.OrderStatus.Closed; break;
                            }
                            Changed?.Invoke(this, new(new Model.FullOrder()
                            {
                                Id = order.Id.ToString(),
                                Price = order.Price.HasValue ? order.Price.Value : default(double),
                                Quantity = order.Size.HasValue ? (int)order.Size.Value : default(int),
                                Signature = signature

                            }));
                        }
                    });
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
    }
}
