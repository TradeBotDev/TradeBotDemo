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
        #region Common Publishers
        public IPublisher<BookLevel> CreateBook25Publisher(BitmexWebsocketClient client,string slot);
        
        public IPublisher<Instrument> CreateInstrumentPublisher(BitmexWebsocketClient client, string slot);

        #endregion
        #region Users Publishres
        public IPublisher<Order> CreateUserOrderPublisher(UserContext context);

        public IPublisher<Margin> CreateUserMarginPublisher(UserContext context);

        public IPublisher<Position> CreateUserPositionPublisher(UserContext context);

        public IPublisher<bool> CreateAuthenticationPublisher(UserContext context);

        public IPublisher<Wallet> CreateWalletPublisher(UserContext context);
        #endregion
    }
}
