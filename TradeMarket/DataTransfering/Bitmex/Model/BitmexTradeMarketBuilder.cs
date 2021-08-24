using Bitmex.Client.Websocket.Client;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.Model.Publishers;
using TradeMarket.Model.TradeMarkets;

namespace TradeMarket.DataTransfering.Bitmex.Model
{
    public class BitmexTradeMarketBuilder : ITradeMarketBuilder
    {
        private BitmexTradeMarket _tradeMarket;

        public BitmexTradeMarketBuilder()
        {
            _tradeMarket = new BitmexTradeMarket();
        }
        public BitmexTradeMarketBuilder(BitmexTradeMarket tm)
        {
            _tradeMarket = tm;
        }

        public TradeMarket.Model.TradeMarkets.TradeMarket Result
        {
            get
            {
                var result = _tradeMarket;
                Reset();
                return result;
            }
        }

        public ITradeMarketBuilder AddCommonClient(BitmexWebsocketClient client)
        {
            _tradeMarket.CommonWSClient = client;
            return this;
        }

        public ITradeMarketBuilder AddCommonClient(BitmexRestfulClient client)
        {
            _tradeMarket.CommonRestClient = client;
            return this;
        }

        public ITradeMarketBuilder AddConnectionMultiplexer(IConnectionMultiplexer multiplexer)
        {
            _tradeMarket.Multiplexer = multiplexer;
            return this;
        }

        public ITradeMarketBuilder AddName(string name)
        {
            _tradeMarket.Name = name;
            return this;
        }

        public ITradeMarketBuilder AddPublisherFactory(IPublisherFactory factory)
        {
            _tradeMarket.PublisherFactory = factory;
            return this;
        }

        public ITradeMarketBuilder ReadErrors(CancellationToken token,ILogger logger)
        {
            var errorPub = _tradeMarket.PublisherFactory.CreateErrorPublisher(_tradeMarket.CommonWSClient, null, token);
            errorPub.Start(logger);
            return this;
        }

        public void Reset()
        {
            _tradeMarket = new BitmexTradeMarket();
        }

        public ITradeMarketBuilder StartPingPong(CancellationToken token,ILogger logger)
        {
            var pingPongPub = _tradeMarket.PublisherFactory.CreatePingPongPublisher(_tradeMarket.CommonWSClient, null, token);
            pingPongPub.Start(logger);
            return this;
        }
    }
}
