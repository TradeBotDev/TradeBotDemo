using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class AuthenticationPublisher : BitmexPublisher<AuthenticationResponse, AuthenticationRequest, bool>
    {
        internal static readonly Action<AuthenticationResponse, EventHandler<IPublisher<bool>.ChangedEventArgs>> _action = (response, e) =>
        {
            e?.Invoke(nameof(AuthenticationPublisher), new(response.Success,BitmexAction.Undefined));
        };

        private IObservable<AuthenticationResponse> _stream;

        public AuthenticationPublisher(BitmexWebsocketClient client, IObservable<AuthenticationResponse> orderStream) : base(client, _action)
        {
            _stream = orderStream;
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public async Task SubcribeAsync(string apiKey, string apiSecret,  CancellationToken token)
        {
            await base.SubscribeAsync(new AuthenticationRequest(apiKey,apiSecret), _stream, token);
        }
    }
}

