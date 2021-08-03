using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Model.Publishers;

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

        #region Parameters For SubscribeAsync

        private CancellationToken _token;
        #endregion

        public UserPositionPublisher(BitmexWebsocketClient client, IObservable<PositionResponse> stream,CancellationToken token) : base(client, _action)
        {
            _stream = stream;

            _token = token;
        }

        public async override Task Start()
        {
            await SubcribeAsync(_token);
        }

        public async Task SubcribeAsync(CancellationToken token)
        {
            await base.SubscribeAsync(new PositionSubscribeRequest(), _stream, token);

        }
    }
}
