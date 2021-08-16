using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class UserPositionPublisher : BitmexPublisher<PositionResponse,PositionSubscribeRequest,Position>
    {
        internal static readonly Action<PositionResponse, EventHandler<IPublisher<Position>.ChangedEventArgs>> _action = (response, e) =>
        {
            foreach (var data in response.Data)
            {
                e?.Invoke(nameof(UserOrderPublisher), new(data, response.Action));
            }
        };

        private IObservable<PositionResponse> _stream;

        public UserPositionPublisher(BitmexWebsocketClient client, IObservable<PositionResponse> stream) : base(client, _action)
        {
            _stream = stream;
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public async Task SubcribeAsync(CancellationToken token)
        {
            await base.SubscribeAsync(new PositionSubscribeRequest(), _stream, token);

        }
    }
}
