using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Books;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Clients;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class BookPublisher : BitmexPublisher<BookResponse, SubscribeRequestBase, BookLevel>
    {

        private IObservable<BookResponse> _stream;
        
        private async static void _OnBookUpdated(BookResponse response, EventHandler<IPublisher<BookLevel>.ChangedEventArgs> e)
        {
            foreach (var data in response.Data)
            {
                e?.Invoke(nameof(BookPublisher), new(data, response.Action));
                await _client.Send($"Bitmex_{data.Symbol}_{data.Id}", data, "Bitmex_Book25");
            }
        }

        private static RedisClient _client;

        public BookPublisher(BitmexWebsocketClient client,IObservable<BookResponse> stream,IConnectionMultiplexer multiplexer) 
            : base(client, _OnBookUpdated)
        {
            _client = new RedisClient(multiplexer);
            _stream = stream;
        }

        public async Task SubscribeAsync(SubscribeRequestBase bookSubscribeRequest,CancellationToken token) 
        {
            await base.SubscribeAsync(bookSubscribeRequest, _stream, token);
        }
    }
}
