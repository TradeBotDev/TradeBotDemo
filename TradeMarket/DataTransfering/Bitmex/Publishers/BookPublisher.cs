using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Responses.Books;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        private static Action<BookResponse, EventHandler<IPublisher<BookLevel>.ChangedEventArgs>,ILogger> _OnBookUpdated = async (response, e,logger) =>
        {
            await Task.Run(async () =>
            {
                var log = logger.ForContext<BookPublisher>();
                try
                {
                    foreach (var data in response.Data)
                    {
                        e?.Invoke(typeof(BookPublisher), new(data, response.Action));
                    log.Information("Response : {@Response}", data);
                    await _redisClient.Send($"Bitmex_{data.Symbol}_{data.Id}", data, "Bitmex_Book25");
                    }
                }catch(Exception e)
                {
                    log.Warning(e.Message);
                    log.Warning(e.StackTrace);
                }
            });
        };

        private static RedisClient _redisClient;

        public BookPublisher(BitmexWebsocketClient client, IObservable<BookResponse> stream, IConnectionMultiplexer multiplexer, SubscribeRequestBase bookSubscribeRequest, CancellationToken token)
            : base(client, _OnBookUpdated)
        {
            _redisClient = new RedisClient(multiplexer);
            _stream = stream;
            this._bookSubscribeRequest = bookSubscribeRequest;
            this._token = token;
        }

        private BookLevel FillCacheFiled(BookLevel oldField, BookLevel newField)
        {
            newField.Price = newField.Price is null || newField.Price == 0 ? oldField.Price : newField.Price;
            newField.Size = newField.Size is null || newField.Size == 0 ? oldField.Size : newField.Size;
            return newField;
        }

        internal class BookComparer : IEqualityComparer<BookLevel>
        {

            public bool Equals(BookLevel x, BookLevel y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode([DisallowNull] BookLevel obj)
            {
                return obj.GetHashCode();
            }
        }

        public override void AddModelToCache(BookResponse response)
        {
            lock (locker)
            {
                switch (response.Action)
                {
                    case BitmexAction.Delete:
                        {
                            _cache.RemoveAll(el => Enumerable.Contains(response.Data, el, new BookComparer()));
                            break;
                        }
                    default:
                        {
                            var changedList =
                                _cache
                                .FindAll(x => x.Id == response.Data.FirstOrDefault(y => x.Id == y.Id)?.Id)
                                .Select(oldField =>
                                    {
                                        var newField = response.Data.First(data => data.Id == oldField.Id);
                                        return FillCacheFiled(oldField, newField);
                                    });
                            _cache.RemoveAll(x => x.Id == response.Data.FirstOrDefault(y => x.Id == y.Id)?.Id);
                            _cache.AddRange(changedList);
                            break;
                        }
                }

            }
        }

        protected async Task SubscribeAsync(SubscribeRequestBase bookSubscribeRequest, CancellationToken token,ILogger logger)
        {
            await base.SubscribeAsync(bookSubscribeRequest, _stream, token,logger);
        }

        public async override Task Start(ILogger logger)
        {
            var log = logger
                .ForContext<BookPublisher>()
                .ForContext("Method", nameof(Start));
            await SubscribeAsync(_bookSubscribeRequest, _token,log);
        }

        public async override Task Stop(ILogger logger)
        {
            var log = logger
                .ForContext<BookPublisher>()
                .ForContext("Method", nameof(Stop));
            await UnSubscribeAsync(_bookSubscribeRequest,log);
            await base.Stop(logger);
            ClearCahce(log);
        }
    }
}
