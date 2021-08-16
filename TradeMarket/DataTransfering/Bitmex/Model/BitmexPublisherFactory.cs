using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Instruments;
using Bitmex.Client.Websocket.Responses.Margins;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Wallets;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Publishers;
using TradeMarket.Model.Publishers;
using TradeMarket.Model.TradeMarkets;
using TradeMarket.Model.UserContexts;
using Order = Bitmex.Client.Websocket.Responses.Orders.Order;

namespace TradeMarket.DataTransfering.Bitmex.Model
{
    public class BitmexPublisherFactory : IPublisherFactory
    {
        private IConnectionMultiplexer _multiplexer;

        public BitmexPublisherFactory(IConnectionMultiplexer multiplexer)
        {
            _multiplexer = multiplexer;
        }

        public IPublisher<bool> CreateAuthenticationPublisher(BitmexWebsocketClient client, IContext context, CancellationToken token)
        {
            return new AuthenticationPublisher(client, client.Streams.AuthenticationStream, context.Key,context.Secret,token);
        }

        public IPublisher<BookLevel> CreateBook25Publisher(BitmexWebsocketClient client, IContext context, CancellationToken token)
        {
            //TODO добавить мультиплексер
            return new BookPublisher(client, client.Streams.Book25Stream,_multiplexer,new Book25SubscribeRequest(context.Signature.SlotName), token);
        }

        public IPublisher<Instrument> CreateInstrumentPublisher(BitmexWebsocketClient client, IContext context, CancellationToken token)
        {
            return new InstrumentPublisher(client, client.Streams.InstrumentStream,context.Signature.SlotName ,token);
        }

        public IPublisher<Margin> CreateUserMarginPublisher(BitmexWebsocketClient client, IContext context, CancellationToken token)
        {
            return new UserMarginPublisher(client, client.Streams.MarginStream, token);
        }

        public IPublisher<Order> CreateUserOrderPublisher(BitmexWebsocketClient client, IContext context, CancellationToken token)
        {
            return new UserOrderPublisher(client, client.Streams.OrderStream, token);
        }

        public IPublisher<Position> CreateUserPositionPublisher(BitmexWebsocketClient client, IContext context, CancellationToken token)
        {
            return new UserPositionPublisher(client, client.Streams.PositionStream, token);
        }

        public IPublisher<Wallet> CreateWalletPublisher(BitmexWebsocketClient client, IContext context, CancellationToken token)
        {
            return new UserWalletPublisher(client, client.Streams.WalletStream, token);
        }
    }
}
