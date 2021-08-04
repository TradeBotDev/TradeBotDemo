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
        public readonly Uri URI;

        private BitmexRestufllLink(string link)
        {
            URI = new Uri(link);
        }

        public static readonly BitmexRestufllLink Bitmex = new BitmexRestufllLink("https://www.bitmex.com");
        public static readonly BitmexRestufllLink Testnet = new BitmexRestufllLink("https://testnet.bitmex.com");
    }

    public class BitmexRestfulClient
    {
        private HttpClient _client;

        public BitmexRestfulClient(BitmexRestufllLink link) 
        {
            _client = new HttpClient() { BaseAddress = link.URI };
        }

        public async Task<BitmexResfulResponse<T>> SendAsync<T>(BitmexRestfulRequest<T> request,CancellationToken cancellationToken)
        {
            return BitmexResfulResponse<T>.Create(await _client.SendAsync(request, cancellationToken));
        }

    }
}
