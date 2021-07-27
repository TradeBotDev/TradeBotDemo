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
    public class BitmexRestfulClient
    {
        private HttpClient _client;
        public static readonly string BitmexUri = "https://testnet.bitmex.com";

        public BitmexRestfulClient() 
        {
            _client = new HttpClient() { BaseAddress = new Uri(BitmexUri) };
        }

        public async Task<BitmexResfulResponse<T>> SendAsync<T>(BitmexRestfulRequest<T> request,CancellationToken cancellationToken)
        {
            return new(await _client.SendAsync(request, cancellationToken));
        }

    }
}
