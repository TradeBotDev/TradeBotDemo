using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class UserWalletPublisher : BitmexPublisher<WalletResponse,WalletSubscribeRequest,Wallet>, IUltimatePublisher
    {
        internal static readonly Action<WalletResponse, EventHandler<IPublisher<Wallet>.ChangedEventArgs>> _action = (response, e) =>
        {
            foreach (var data in response.Data)
            {
                e?.Invoke(nameof(UserOrderPublisher), new(data));
            }
        };

        private IObservable<WalletResponse> _stream;

        public UserWalletPublisher(IObservable<WalletResponse> stream) : base(_action)
        {
            _stream = stream;
        }

        public async Task SubcribeAsync(CancellationToken token)
        {
            await base.SubscribeAsync(new WalletSubscribeRequest(), _stream, token);

        }
    }
}
