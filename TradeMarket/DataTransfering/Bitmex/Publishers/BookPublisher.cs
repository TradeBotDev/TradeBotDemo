using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class BookPublisher : BitmexPublisher<BookResponse, SubscribeRequestBase, BookLevel>
    {

        private IObservable<BookResponse> _stream;

        public BookPublisher(BitmexWebsocketClient client,IObservable<BookResponse> stream) 
            : base(client, (response, e) =>
                {
                    foreach (var data in response.Data)
                    {
                        e?.Invoke(nameof(BookPublisher), new(data, response.Action));
                    }
                })
        {
            _stream = stream;
        }

        public async Task SubscribeAsync(SubscribeRequestBase bookSubscribeRequest,CancellationToken token) 
        {
            await base.SubscribeAsync(bookSubscribeRequest, _stream, token);
        }
    }
}
