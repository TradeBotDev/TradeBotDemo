using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses;
using Serilog;
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
        internal static readonly Action<AuthenticationResponse, EventHandler<IPublisher<bool>.ChangedEventArgs>,ILogger> _action = async (response, e,logger) =>
        {
           await Task.Run(() =>
           {
                var log = logger.ForContext<AuthenticationPublisher>();
               try
               {
                    log.Information("Response : @{Response}", response);
                    e?.Invoke(typeof(AuthenticationPublisher), new(response.Success, BitmexAction.Undefined));
               }catch(Exception e)
               {
                   log.Warning(e.Message);
                   log.Warning(e.StackTrace);
               }
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

        public async override Task Start(ILogger logger)
        {
            await SubscribeAsync(_apiKey, _apiSecret, _token,logger);
        }

        public async Task SubscribeAsync(string apiKey, string apiSecret,  CancellationToken token,ILogger logger)
        {
            await base.SubscribeAsync(new AuthenticationRequest(apiKey,apiSecret), _stream, token,logger);
        }

        public override void AddModelToCache(AuthenticationResponse response)
        {
            lock (base.locker)
            {
                _cache.Add(response.Success);
            }
        }

        public async override Task Stop(ILogger logger)
        {
            await Task.Run(() => ClearCahce(logger));
        }
    }
}

