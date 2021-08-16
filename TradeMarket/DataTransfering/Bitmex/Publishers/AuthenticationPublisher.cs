using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Model.Publishers;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class AuthenticationPublisher : BitmexPublisher<AuthenticationResponse, AuthenticationRequest, bool>
    {
        internal static readonly Action<AuthenticationResponse, EventHandler<IPublisher<bool>.ChangedEventArgs>> _action = async (response, e) =>
        {
           await Task.Run(() =>
           {
               e?.Invoke(typeof(AuthenticationPublisher), new(response.Success, BitmexAction.Undefined));
           });
        };



        private IObservable<AuthenticationResponse> _stream;
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly CancellationToken _token;

        public AuthenticationPublisher(BitmexWebsocketClient client, IObservable<AuthenticationResponse> stream, string apiKey, string apiSecret, CancellationToken token) 
            : base(client, _action)
        {
            _stream = stream;
            this._apiKey = apiKey;
            this._apiSecret = apiSecret;
            this._token = token;
        }

        public async override Task Start()
        {
            await SubscribeAsync(_apiKey, _apiSecret, _token);
        }

        public async Task SubscribeAsync(string apiKey, string apiSecret,  CancellationToken token)
        {
            await base.SubscribeAsync(new AuthenticationRequest(apiKey,apiSecret), _stream, token);
        }
    }
}

