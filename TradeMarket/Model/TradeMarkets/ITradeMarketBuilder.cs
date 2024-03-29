﻿using Bitmex.Client.Websocket.Client;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Model;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.Model.Publishers;
using TradeMarket.Model.UserContexts;

namespace TradeMarket.Model.TradeMarkets
{
    public interface ITradeMarketBuilder
    {
        public ITradeMarketBuilder AddName(string name);

        public ITradeMarketBuilder AddPublisherFactory(IPublisherFactory factory);

        public ITradeMarketBuilder AddCommonClient(BitmexWebsocketClient client);

        public ITradeMarketBuilder AddCommonClient(RestfulClient client);

        public ITradeMarketBuilder AddConnectionMultiplexer(IConnectionMultiplexer multiplexer);

        public ITradeMarketBuilder StartPingPong(CancellationToken token,ILogger logger);

        public ITradeMarketBuilder ReadErrors(CancellationToken token, ILogger logger);

        public void Reset();

        public TradeMarkets.TradeMarket Result { get; }

    }
}
