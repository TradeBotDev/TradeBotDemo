using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Instruments;
using Bitmex.Client.Websocket.Responses.Margins;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.DataTransfering;
using TradeMarket.Model.UserContexts;

namespace TradeMarket.Model.Publishers
{
    public interface IPublisherFactory
    {
        public delegate IPublisher<T> Create<T>(BitmexWebsocketClient client, UserContext context);

        #region Common Publishers
        public IPublisher<BookLevel> CreateBook25Publisher(BitmexWebsocketClient client, UserContext context);
        
        public IPublisher<Instrument> CreateInstrumentPublisher(BitmexWebsocketClient client, UserContext context);

        #endregion
        #region Users Publishres
        public IPublisher<Order> CreateUserOrderPublisher(BitmexWebsocketClient client,UserContext context);

        public IPublisher<Margin> CreateUserMarginPublisher(BitmexWebsocketClient client,UserContext context);

        public IPublisher<Position> CreateUserPositionPublisher(BitmexWebsocketClient client,UserContext context);

        public IPublisher<bool> CreateAuthenticationPublisher(BitmexWebsocketClient client,UserContext context);

        public IPublisher<Wallet> CreateWalletPublisher(BitmexWebsocketClient client,UserContext context);
        #endregion
    }
}
