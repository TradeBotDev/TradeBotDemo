using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Instruments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Model.Publishers;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class InstrumentPublisher : BitmexPublisher<InstrumentResponse, InstrumentSubscribeRequest, Instrument>
    {
        internal static readonly Action<InstrumentResponse, EventHandler<IPublisher<Instrument>.ChangedEventArgs>> _action = async (response, e) =>
        {
            await Task.Run(() =>
           {
               foreach (var data in response.Data)
               {
                   e?.Invoke(nameof(UserOrderPublisher), new(data, response.Action));
               }
           });
        };

        private IObservable<InstrumentResponse> _stream;
        private readonly string _slot;
        private readonly CancellationToken _token;

        public InstrumentPublisher(BitmexWebsocketClient client, IObservable<InstrumentResponse> stream, string slot, CancellationToken token) : base(client, _action)
        {
            _stream = stream;
            this._slot = slot;
            this._token = token;
        }

        public async override Task Start()
        {
            await SubscribeAsync(_slot,_token);
        }

        public async Task SubscribeAsync(string slot,CancellationToken token)
        {
            await base.SubscribeAsync(new InstrumentSubscribeRequest(slot),_stream, token);

        }
    }
}
