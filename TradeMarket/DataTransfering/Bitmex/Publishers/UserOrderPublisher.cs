using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class UserOrderPublisher : BitmexPublisher<OrderResponse,OrderSubscribeRequest,Order>
    {
        internal static readonly Action<OrderResponse, EventHandler<IPublisher<Order>.ChangedEventArgs>> _action = (response, e) =>
        {
            foreach (var data in response.Data)
            {
                e?.Invoke(nameof(UserOrderPublisher), new(data, response.Action));
            }
        };

        private IObservable<OrderResponse> _stream;

        public UserOrderPublisher(BitmexWebsocketClient client,IObservable<OrderResponse> orderStream) : base(client,_action)
        {
            _stream = orderStream;
        }

        public async Task SubcribeAsync(CancellationToken token)
        {
            await base.SubscribeAsync(new OrderSubscribeRequest(), _stream, token);
        }
    }
}
