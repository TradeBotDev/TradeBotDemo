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
    public class ErrorPublisher : BitmexPublisher<ErrorResponse, RequestBase, string>
    {
        internal static readonly Action<ErrorResponse, EventHandler<IPublisher<string>.ChangedEventArgs>> _action = async (response, e) =>
        {
            await Task.Run(() =>
            {
                Log.Error("Error {@Error} from request {@Request}", response.Error, response.Request);
                e?.Invoke(nameof(UserOrderPublisher), new(response.Error, BitmexAction.Insert));
            });
        };

        private IObservable<ErrorResponse> _stream;

        #region Parameters For SubscribeAsync

        private CancellationToken _token;
        #endregion

        public ErrorPublisher(BitmexWebsocketClient client, IObservable<ErrorResponse> stream, CancellationToken token) : base(client, _action)
        {
            _stream = stream;

            _token = token;
        }

        public override void AddModelToCache(ErrorResponse response)
        {
            _cache.Add(response.Error);
        }

        public async override Task Start()
        {
            await SubscribeAsync(_token);
        }

        public async Task SubscribeAsync(CancellationToken token)
        {
            await base.SubscribeAsync(null,_stream, token);

        }
    }
}
