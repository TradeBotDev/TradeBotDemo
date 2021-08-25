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

        private BitmexWebsocketClient _client;


        public BitmexPublisherFactory(IConnectionMultiplexer multiplexer,BitmexWebsocketClient client)
        {
            _client = client;
            _multiplexer = multiplexer;
        }

        public IPublisher<bool> CreateAuthenticationPublisher(Context context, CancellationToken token)
        {
            var con = context as BitmexContext;
            return new AuthenticationPublisher(con.WSClient, con.WSClient.Streams.AuthenticationStream, context.Key,context.Secret,token);
        }

        public IPublisher<BookLevel> CreateBook25Publisher(Context context, CancellationToken token)
        {
            
            return new BookPublisher(_client, _client.Streams.Book25Stream,_multiplexer,new Book25SubscribeRequest(context.Signature.SlotName), token);
        }

        public IPublisher<string> CreateErrorPublisher(Context context, CancellationToken token)
        {
            return new ErrorPublisher(_client, _client.Streams.ErrorStream, token);
        }

        public IPublisher<Instrument> CreateInstrumentPublisher(Context context, CancellationToken token)
        {
            var con = context as BitmexContext; 
            return new InstrumentPublisher(con.WSClient, con.WSClient.Streams.InstrumentStream,new(context.Signature.SlotName),token);
        }

        public IPublisher<string> CreatePingPongPublisher( Context context, CancellationToken token)
        {
            return new PingPongPublisher(_client, _client.Streams.PongStream,token);
        }

        public IPublisher<Margin> CreateUserMarginPublisher(Context context, CancellationToken token)
        {
            var con = context as BitmexContext;
            return new UserMarginPublisher(con.WSClient, con.WSClient.Streams.MarginStream,new MarginSubscribeRequest(), token);
        }

        public IPublisher<Order> CreateUserOrderPublisher(Context context, CancellationToken token)
        {
            var con = context as BitmexContext;
            return new UserOrderPublisher(con.WSClient, con.WSClient.Streams.OrderStream,new OrderSubscribeRequest(), token);
        }

        public IPublisher<Position> CreateUserPositionPublisher(Context context, CancellationToken token)
        {
            var con = context as BitmexContext;
            return new UserPositionPublisher(con.WSClient, con.WSClient.Streams.PositionStream,new PositionSubscribeRequest(), token);
        }

        public IPublisher<Wallet> CreateWalletPublisher(Context context, CancellationToken token)
        {
            var con = context as BitmexContext;
            return new UserWalletPublisher(con.WSClient, con.WSClient.Streams.WalletStream,new WalletSubscribeRequest(), token);
        }
    }
}
