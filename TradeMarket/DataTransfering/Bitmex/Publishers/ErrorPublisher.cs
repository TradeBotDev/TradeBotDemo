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
        internal static readonly Action<ErrorResponse, EventHandler<IPublisher<string>.ChangedEventArgs>,ILogger> _action = async (response, e,logger) =>
        {
            var log = logger.ForContext<ErrorPublisher>();
            await Task.Run(() =>
            {
                try
                {
                    log.Error("Error {@Error} from request {@Request}", response.Error, response.Request);
                    e?.Invoke(nameof(UserOrderPublisher), new(response.Error, BitmexAction.Insert));
                }
                catch (Exception e)
                {
                    log.Warning(e.Message);
                    log.Warning(e.StackTrace);
                }
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
            lock (locker)
            {
                _cache.Add(response.Error);
            }
        }

        public async override Task Start(ILogger logger)
        {
            var log = logger.ForContext<ErrorPublisher>();
            await SubscribeAsync(_token,log);
        }

        public async override Task Stop(ILogger logger)
        {
            var log = logger.ForContext<ErrorPublisher>();
            await base.Stop(logger);
            ClearCahce(log);
        }

        public async Task SubscribeAsync(CancellationToken token,ILogger logger)
        {
            await base.SubscribeAsync(null,_stream, token,logger);
        }

    }
}
