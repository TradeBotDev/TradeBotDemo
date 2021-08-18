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
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.DataTransfering;
using TradeMarket.Model.UserContexts;

namespace TradeMarket.Model.Publishers
{
    public interface IPublisherFactory
    {
        public delegate IPublisher<T> Create<T>(BitmexWebsocketClient client, IContext context, CancellationToken token);

        #region Common Publishers
        public IPublisher<BookLevel> CreateBook25Publisher(BitmexWebsocketClient client, IContext context, CancellationToken token);
        
        public IPublisher<Instrument> CreateInstrumentPublisher(BitmexWebsocketClient client, IContext context, CancellationToken token);

        #endregion
        #region Users Publishres
        public IPublisher<Order> CreateUserOrderPublisher(BitmexWebsocketClient client, IContext context, CancellationToken token);

        public IPublisher<Margin> CreateUserMarginPublisher(BitmexWebsocketClient client, IContext context, CancellationToken token);

        public IPublisher<Position> CreateUserPositionPublisher(BitmexWebsocketClient client, IContext context, CancellationToken token);

        public IPublisher<bool> CreateAuthenticationPublisher(BitmexWebsocketClient client, IContext context, CancellationToken token);

        public IPublisher<Wallet> CreateWalletPublisher(BitmexWebsocketClient client, IContext context, CancellationToken token);

        public IPublisher<string> CreatePingPongPublisher(BitmexWebsocketClient client, IContext context, CancellationToken token);

        public IPublisher<string> CreateErrorPublisher(BitmexWebsocketClient client, IContext context, CancellationToken token);
        #endregion
    }
}
