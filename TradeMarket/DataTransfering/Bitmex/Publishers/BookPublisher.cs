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
    //TODO как проверить что SubscribeRequestBase относится к книгам? Опять переписывать исходики ???
    public class BookPublisher : BitmexPublisher<BookResponse, SubscribeRequestBase, BookLevel>
    {

        internal static readonly Action<BookResponse, EventHandler<IPublisher<BookLevel>.ChangedEventArgs>> _action = (response, e) =>
        {
            foreach (var data in response.Data)
            {
                e?.Invoke(nameof(BookPublisher), new(data));
            }
        };
        private IObservable<BookResponse> _bookStream;
        private String _slotName;
        private SubscribeRequestBase _bookSubscribeRequest;
        public BookPublisher(IObservable<BookResponse> bookStream,String slotName,SubscribeRequestBase bookSubscribeRequest) : base(_action)
        {
            _slotName = slotName;
            _bookStream = bookStream;
            _bookSubscribeRequest = bookSubscribeRequest;
        }

        public async Task SubscribeAsync(CancellationToken token)
        {
            await base.SubscribeAsync(_bookSubscribeRequest, _bookStream, token);
        }
    }
}
