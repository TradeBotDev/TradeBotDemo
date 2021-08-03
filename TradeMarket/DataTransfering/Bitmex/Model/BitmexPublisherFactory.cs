using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
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
using TradeMarket.DataTransfering.Bitmex.Publishers;
using TradeMarket.Model.Publishers;
using TradeMarket.Model.TradeMarkets;
using TradeMarket.Model.UserContexts;

namespace TradeMarket.DataTransfering.Bitmex.Model
{
    public class BitmexPublisherFactory : IPublisherFactory
    {
        public IPublisher<bool> CreateAuthenticationPublisher(UserContext context)
        {
            return new AuthenticationPublisher(context.WSClient, context.WSClient.Streams.AuthenticationStream, context.Key,context.Secret,new System.Threading.CancellationToken());
        }

        public IPublisher<BookLevel> CreateBook25Publisher(BitmexWebsocketClient client, string slot)
        {
            //TODO добавить мультиплексер
            return new BookPublisher(client, client.Streams.Book25Stream,null,new Book25SubscribeRequest(slot), new System.Threading.CancellationToken());
        }

        public IPublisher<Instrument> CreateInstrumentPublisher(BitmexWebsocketClient client, string slot)
        {
            return new InstrumentPublisher(client, client.Streams.InstrumentStream,slot ,new System.Threading.CancellationToken());
        }

        public IPublisher<Margin> CreateUserMarginPublisher(UserContext context)
        {
            return new UserMarginPublisher(context.WSClient, context.WSClient.Streams.MarginStream, new System.Threading.CancellationToken());
        }

        public IPublisher<Order> CreateUserOrderPublisher(UserContext context)
        {
            return new UserOrderPublisher(context.WSClient, context.WSClient.Streams.OrderStream, new System.Threading.CancellationToken());
        }

        public IPublisher<Position> CreateUserPositionPublisher(UserContext context)
        {
            return new UserPositionPublisher(context.WSClient, context.WSClient.Streams.PositionStream, new System.Threading.CancellationToken());
        }

        public IPublisher<Wallet> CreateWalletPublisher(UserContext context)
        {
            return new UserWalletPublisher(context.WSClient, context.WSClient.Streams.WalletStream, new System.Threading.CancellationToken());
        }
    }
}
