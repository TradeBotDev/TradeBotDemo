using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Margins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Model.Publishers;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class UserMarginPublisher : BitmexPublisher<MarginResponse,MarginSubscribeRequest, Margin>
    {
        internal static readonly Action<MarginResponse, EventHandler<IPublisher<Margin>.ChangedEventArgs>> _action = (response, e) =>
        {
            foreach (var data in response.Data)
            {
                e?.Invoke(nameof(UserOrderPublisher), new(data, response.Action));
            }
        };

        private IObservable<MarginResponse> _stream;
        private readonly CancellationToken _token;

        public UserMarginPublisher(BitmexWebsocketClient client, IObservable<MarginResponse> stream, CancellationToken token) : base(client, _action)
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
            await base.SubscribeAsync(new MarginSubscribeRequest(), _stream, token);

        }
    }
}
