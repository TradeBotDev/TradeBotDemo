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
        public static readonly string BitmexUri = "https://www.bitmex.com";

        public BitmexRestfulClient() 
        {
            _client = new HttpClient() { BaseAddress = new Uri(BitmexUri) };
        }

        public async Task<HttpResponseMessage> SendAsync(BitmexRestfulRequest request,CancellationToken cancellationToken)
        {
            return await _client.SendAsync(request, cancellationToken);
        }

    }
}
