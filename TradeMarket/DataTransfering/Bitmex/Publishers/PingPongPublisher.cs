using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Model.Publishers;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class PingPongPublisher : BitmexPublisher< PongResponse, PingRequest, string>
    {
        internal static readonly Action<PongResponse, EventHandler<IPublisher<string>.ChangedEventArgs>,ILogger> _action = async (response, e,logger) =>
        {
            await Task.Run(() =>
            {
                var log = logger.ForContext<PingPongPublisher>();
                log.Information("Response : {@Response}", response);
                e?.Invoke(nameof(UserOrderPublisher), new(response.Message, BitmexAction.Insert));
            });
        };

        private IObservable<PongResponse> _stream;
        private readonly CancellationToken _token;

        public PingPongPublisher(BitmexWebsocketClient client, IObservable<PongResponse> stream, CancellationToken token) : base(client, _action)
        {
            _stream = stream;
            this._token = token;
        }

        public async override Task Start(ILogger logger)
        {
            var log = logger.ForContext<PingPongPublisher>();
            await SubscribeAsync( _token,log);
        }

        public async Task SubscribeAsync(CancellationToken token,ILogger logger)
        {
            var log = logger.ForContext("Method", nameof(SubscribeAsync));
            await base.SubscribeAsync(null, _stream, token,logger);
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(5_000);
                log.Information("Ping");
                base._client.Send(new PingRequest());
            }

        }

        public override void AddModelToCache(PongResponse response)
        {
            lock (locker)
            {
                _cache.Add(response.Message);
            }
        }

        public async override Task Stop(ILogger logger)
        {
            var log = logger.ForContext<PingPongPublisher>();
            ClearCahce(log);
        }
    }
}
