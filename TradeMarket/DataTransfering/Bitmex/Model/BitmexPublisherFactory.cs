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
        public IPublisher<bool> CreateAuthenticationPublisher(BitmexWebsocketClient client, UserContext context)
        {
            return new AuthenticationPublisher(context.WSClient, context.WSClient.Streams.AuthenticationStream, context.Key,context.Secret,new System.Threading.CancellationToken());
        }

        public IPublisher<BookLevel> CreateBook25Publisher(BitmexWebsocketClient client, UserContext context)
        {
            //TODO добавить мультиплексер
            return new BookPublisher(client, client.Streams.Book25Stream,null,new Book25SubscribeRequest(context.SlotName), new System.Threading.CancellationToken());
        }

        public IPublisher<Instrument> CreateInstrumentPublisher(BitmexWebsocketClient client, UserContext context)
        {
            return new InstrumentPublisher(client, client.Streams.InstrumentStream,context.SlotName ,new System.Threading.CancellationToken());
        }

        public IPublisher<Margin> CreateUserMarginPublisher(BitmexWebsocketClient client,UserContext context)
        {
            return new UserMarginPublisher(context.WSClient, context.WSClient.Streams.MarginStream, new System.Threading.CancellationToken());
        }

        public IPublisher<Order> CreateUserOrderPublisher(BitmexWebsocketClient client,UserContext context)
        {
            return new UserOrderPublisher(context.WSClient, context.WSClient.Streams.OrderStream, new System.Threading.CancellationToken());
        }

        public IPublisher<Position> CreateUserPositionPublisher(BitmexWebsocketClient client,UserContext context)
        {
            return new UserPositionPublisher(context.WSClient, context.WSClient.Streams.PositionStream, new System.Threading.CancellationToken());
        }

        public IPublisher<Wallet> CreateWalletPublisher(BitmexWebsocketClient client, UserContext context)
        {
            return new UserWalletPublisher(context.WSClient, context.WSClient.Streams.WalletStream, new System.Threading.CancellationToken());
        }
    }
}
