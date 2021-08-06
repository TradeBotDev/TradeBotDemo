using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Instruments;
using Bitmex.Client.Websocket.Responses.Margins;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Responses.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.DataTransfering;

namespace TradeMarket.Model
{
    public interface IPublisherFactory
    {
        public IPublisher<BookLevel> CreateBook25Publisher(UserContext context);

        public IPublisher<Order> CreateUserOrderPublisher(UserContext context);

        public IPublisher<Margin> CreateUserMarginPublisher(UserContext context);

        public IPublisher<Position> CreateUserPositionPublisher(UserContext context);

        public IPublisher<Instrument> CreateInstrumentPublisher(UserContext context);

        public IPublisher<bool> CreateAuthenticationPublisher(UserContext context);
    }
}
