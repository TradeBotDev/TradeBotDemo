using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Instruments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using TradeMarket.Model.Publishers;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class InstrumentPublisher : BitmexPublisher<InstrumentResponse, InstrumentSubscribeRequest, Instrument>
    {
        internal static readonly Action<InstrumentResponse, EventHandler<IPublisher<Instrument>.ChangedEventArgs>> _action = async (response, e) =>
        {
            await Task.Run(() =>
           {
               try
               {
                   foreach (var data in response.Data)
                   {
                       e?.Invoke(nameof(UserOrderPublisher), new(data, response.Action));
                   }
               }
               catch(Exception e)
               {
                   Log.Warning(e.Message);
                   Log.Warning(e.StackTrace);
               }
           });
        };
        private InstrumentSubscribeRequest _request;
        private IObservable<InstrumentResponse> _stream;
        private readonly CancellationToken _token;

        public InstrumentPublisher(BitmexWebsocketClient client, IObservable<InstrumentResponse> stream, InstrumentSubscribeRequest request, CancellationToken token) : base(client, _action)
        {
            _request = request;
            _stream = stream;
            this._token = token;
        }

        public override void AddModelToCache(InstrumentResponse response)
        {
            lock (base.locker)
            {
                _cache.Clear();
                _cache.AddRange(response.Data);
            }
        }

        public async override Task Start()
        {
            await SubscribeAsync(_token);
        }

        public async Task SubscribeAsync(CancellationToken token)
        {
            await base.SubscribeAsync(_request,_stream, token);

        }

        public async override Task Stop()
        {
            await UnSubscribeAsync(_request);
        }
    }
}
