using Bitmex.Client.Websocket.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests;
using TradeMarket.DataTransfering.Bitmex.Rest.Responses;

namespace TradeMarket.DataTransfering.Bitmex.Rest.Client
{
    public class BitmexRestufllLink
    {
        public static readonly Uri Bitmex = new Uri("https://www.bitmex.com");
        public static readonly Uri Testnet = new Uri("https://testnet.bitmex.com");
    }

    public class RestfulClient
    {
        private HttpClient _client;

        public RestfulClient(Uri link) 
        {
            _client = new HttpClient() { BaseAddress = link };
        }

        public async Task<ResfulResponse<T>> SendAsync<T>(RestfulRequest<T> request,CancellationToken cancellationToken)
        {
            return await ResfulResponse<T>.Create(await _client.SendAsync(request, cancellationToken));
        }

    }
}
