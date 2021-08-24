using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Responses.Wallets;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Model.Publishers;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class UserWalletPublisher : BitmexPublisher<WalletResponse,WalletSubscribeRequest,Wallet>
    {
        internal static readonly Action<WalletResponse, EventHandler<IPublisher<Wallet>.ChangedEventArgs>,ILogger> _action = async (response, e,logger) =>
        {
            await Task.Run(() =>
            {
                var log = logger.ForContext<UserWalletPublisher>();
                foreach (var data in response.Data)
                {
                    log.Information("Response : {@Response}", data);
                    e?.Invoke(nameof(UserOrderPublisher), new(data, response.Action));
                }
            });
        };
        private WalletSubscribeRequest _request;
        private IObservable<WalletResponse> _stream;
        private readonly CancellationToken _token;

        public UserWalletPublisher(BitmexWebsocketClient client,IObservable<WalletResponse> stream, WalletSubscribeRequest walletSubscribeRequest, CancellationToken token) : base(client,_action)
        {
            _request = walletSubscribeRequest;
            _stream = stream;
            this._token = token;
        }

        public async override Task Start(ILogger logger)
        {
            var log = logger.ForContext<UserWalletPublisher>();
            await SubscribeAsync(_token,log);
        }

        public async Task SubscribeAsync(CancellationToken token,ILogger logger)
        {
            await base.SubscribeAsync(_request, _stream, token,logger);

        }

        public override void AddModelToCache(WalletResponse response)
        {
            lock (locker)
            {
                foreach(var el in response.Data)
                {
                    switch (response.Action)
                    {
                        case BitmexAction.Delete:
                            {
                                _cache.Clear();
                                break;
                            }
                        case BitmexAction.Update:
                            {
                                var model = _cache[0];
                                el.Amount = el.Amount is null || el.Amount == 0 ? model.Amount : el.Amount;
                                _cache.Add(el);
                                break;
                            }
                        default:
                            {
                                _cache.Add(el);
                                break;
                            }
                    }

                }
            }
        }

        public async override Task Stop(ILogger logger)
        {
            var log = logger.ForContext<UserWalletPublisher>();
            await UnSubscribeAsync(_request,log);
            ClearCahce(log);
        }
    }
}
