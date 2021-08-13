using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Wallets;
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
        internal static readonly Action<WalletResponse, EventHandler<IPublisher<Wallet>.ChangedEventArgs>> _action = async (response, e) =>
        {
            await Task.Run(() =>
            {
                foreach (var data in response.Data)
                {
                    e?.Invoke(nameof(UserOrderPublisher), new(data, response.Action));
                }
            });
        };

        private IObservable<WalletResponse> _stream;
        private readonly CancellationToken _token;

        public UserWalletPublisher(BitmexWebsocketClient client,IObservable<WalletResponse> stream, CancellationToken token) : base(client,_action)
        {
            _stream = stream;
            this._token = token;
        }

        public async override Task Start()
        {
            await SubscribeAsync(_token);
        }

        public async Task SubscribeAsync(CancellationToken token)
        {
            await base.SubscribeAsync(new WalletSubscribeRequest(), _stream, token);

        }
    }
}
