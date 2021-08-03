using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Responses.Orders;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Model.Publishers;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class UserOrderPublisher : BitmexPublisher<OrderResponse,OrderSubscribeRequest,Order>
    {
        internal static readonly Action<OrderResponse, EventHandler<IPublisher<Order>.ChangedEventArgs>> _action = (response, e) =>
        {
            foreach (var data in response.Data)
            {
                //при исполнении ордера с биржи прилетает не делит а апдейт
                BitmexAction action = response.Action;
                if(data.Price is null && data.OrderQty is null)
                {
                    action = BitmexAction.Delete;
                }
                Log.Information("{@Where} {@OrderId} {@OrderQuantity} @{OrderPrice} @{OrderAction}", "Trademarket", data.OrderId, data.OrderQty, data.Price, action);
                e?.Invoke(nameof(UserOrderPublisher), new(data, action));
            }
        };

        private IObservable<OrderResponse> _stream;
        private readonly CancellationToken _token;

        public UserOrderPublisher(BitmexWebsocketClient client,IObservable<OrderResponse> stream,CancellationToken token) : base(client,_action)
        {
            _stream = stream;
            this._token = token;
        }

        public async override Task Start()
        {
            await SubcribeAsync(_token);
        }

        public async Task SubcribeAsync(CancellationToken token)
        {
            await base.SubscribeAsync(new OrderSubscribeRequest(), _stream, token);
        }
    }
}
