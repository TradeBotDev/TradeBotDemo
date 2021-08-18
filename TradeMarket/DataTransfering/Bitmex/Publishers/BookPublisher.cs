using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Books;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Clients;
using TradeMarket.Model.Publishers;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class BookPublisher : BitmexPublisher<BookResponse, SubscribeRequestBase, BookLevel>
    {

        private IObservable<BookResponse> _stream;
        private readonly SubscribeRequestBase _bookSubscribeRequest;
        private readonly CancellationToken _token;

        private static Action<BookResponse, EventHandler<IPublisher<BookLevel>.ChangedEventArgs>> _OnBookUpdated = async (response, e) =>
        {
            await Task.Run(async () =>
            {
                foreach (var data in response.Data)
                {
                    e?.Invoke(typeof(BookPublisher), new(data, response.Action));
                    Log.Information("Get {@Info}", data);
                    await _redisClient.Send($"Bitmex_{data.Symbol}_{data.Id}", data, "Bitmex_Book25");
                }
            });
        };

        private static RedisClient _redisClient;

        public BookPublisher(BitmexWebsocketClient client,IObservable<BookResponse> stream,IConnectionMultiplexer multiplexer, SubscribeRequestBase bookSubscribeRequest, CancellationToken token) 
            : base(client, _OnBookUpdated)
        {
            _redisClient = new RedisClient(multiplexer);
            _stream = stream;
            this._bookSubscribeRequest = bookSubscribeRequest;
            this._token = token;
        }

        public override void AddModelToCache(BookResponse response)
        {
            Parallel.ForEach(response.Data, (el) => {
                var model = _cache.First(x => x.Id == el.Id);
                if (model is not null)
                {
                    _cache.Remove(model);
                }
                _cache.Add(el);
            });
        }

        protected async Task SubscribeAsync(SubscribeRequestBase bookSubscribeRequest,CancellationToken token) 
        {
            await base.SubscribeAsync(bookSubscribeRequest, _stream, token);
        }

        public async override Task Start()
        {
            await SubscribeAsync(_bookSubscribeRequest, _token);
        }
    }
}
