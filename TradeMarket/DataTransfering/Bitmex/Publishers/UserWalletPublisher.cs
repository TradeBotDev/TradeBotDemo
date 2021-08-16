using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class UserWalletPublisher : BitmexPublisher<WalletResponse,WalletSubscribeRequest,Wallet>
    {
        internal static readonly Action<WalletResponse, EventHandler<IPublisher<Wallet>.ChangedEventArgs>> _action = (response, e) =>
        {
            foreach (var data in response.Data)
            {
                e?.Invoke(nameof(UserOrderPublisher), new(data, response.Action));
            }
        };

        private IObservable<WalletResponse> _stream;

        public UserWalletPublisher(BitmexWebsocketClient client,IObservable<WalletResponse> stream) : base(client,_action)
        {
            _stream = stream;
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public async Task SubcribeAsync(CancellationToken token)
        {
            await base.SubscribeAsync(new WalletSubscribeRequest(), _stream, token);

        }
    }
}
