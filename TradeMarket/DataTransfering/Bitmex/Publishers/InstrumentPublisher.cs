using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Instruments;
using Serilog;
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
        internal static readonly Action<InstrumentResponse, EventHandler<IPublisher<Instrument>.ChangedEventArgs>,ILogger> _action = async (response, e,logger) =>
        {
            await Task.Run(() =>
           {
              var log = logger.ForContext<InstrumentPublisher>();
               try
               {
                   foreach (var data in response.Data)
                   {
                        log.Information("Response : {@Response}", data);
                       e?.Invoke(nameof(UserOrderPublisher), new(data, response.Action));
                   }
               }
               catch(Exception e)
               {
                   log.Warning(e.Message);
                   log.Warning(e.StackTrace);
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

        public async override Task Start(ILogger logger)
        {
            var log = logger.ForContext<InstrumentPublisher>();
            await SubscribeAsync(_token,log);
        }

        public async Task SubscribeAsync(CancellationToken token,ILogger logger)
        {
            await base.SubscribeAsync(_request,_stream, token,logger);

        }

        public async override Task Stop(ILogger logger)
        {
            var log = logger.ForContext<InstrumentPublisher>();
            await UnSubscribeAsync(_request,log);
        }
    }
}
