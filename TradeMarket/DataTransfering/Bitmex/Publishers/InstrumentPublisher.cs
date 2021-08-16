using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Instruments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class InstrumentPublisher : BitmexPublisher<InstrumentResponse, InstrumentSubscribeRequest, Instrument>
    {
        internal static readonly Action<InstrumentResponse, EventHandler<IPublisher<Instrument>.ChangedEventArgs>> _action = (response, e) =>
        {
            foreach (var data in response.Data)
            {
                e?.Invoke(nameof(UserOrderPublisher), new(data, response.Action));
            }
        };

        private IObservable<InstrumentResponse> _stream;

        public InstrumentPublisher(BitmexWebsocketClient client, IObservable<InstrumentResponse> stream) : base(client, _action)
        {
            _stream = stream;
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public async Task SubcribeAsync(string slot,CancellationToken token)
        {
            await base.SubscribeAsync(new InstrumentSubscribeRequest(slot),_stream, token);

        }
    }
}
