using Bitmex.Client.Websocket.Client;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.Model.Publishers;

namespace TradeMarket.Model.TradeMarkets
{
    public interface ITradeMarketBuilder
    {
        public ITradeMarketBuilder AddName(string name);

        public ITradeMarketBuilder AddPublisherFactory(IPublisherFactory factory);

        public ITradeMarketBuilder AddCommonClient(BitmexWebsocketClient client);

        public ITradeMarketBuilder AddCommonClient(BitmexRestfulClient client);

        public ITradeMarketBuilder AddConnectionMultiplexer(IConnectionMultiplexer multiplexer);

        public ITradeMarketBuilder StartPingPong(CancellationToken token);

        public ITradeMarketBuilder ReadErrors(CancellationToken token);

        public void Reset();

        public TradeMarket Result { get; }

    }
}
