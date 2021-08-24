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
        internal static readonly Action<PongResponse, EventHandler<IPublisher<string>.ChangedEventArgs>> _action = async (response, e) =>
        {
            await Task.Run(() =>
            {
                try
                {
                    e?.Invoke(nameof(UserOrderPublisher), new(response.Message, BitmexAction.Insert));

                }
                catch (Exception e)
                {
                    Log.Warning(e.Message);
                    Log.Warning(e.StackTrace);
                } 
            });
        };

        private IObservable<PongResponse> _stream;
        private readonly CancellationToken _token;

        public PingPongPublisher(BitmexWebsocketClient client, IObservable<PongResponse> stream, CancellationToken token) : base(client, _action)
        {
            _stream = stream;
            this._token = token;
        }

        public async override Task Start()
        {
            await SubscribeAsync( _token);
        }

        public async Task SubscribeAsync(CancellationToken token)
        {
            await base.SubscribeAsync(null, _stream, token);
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(5_000);
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

        public async override Task Stop()
        {
            ClearCahce();
        }
    }
}
