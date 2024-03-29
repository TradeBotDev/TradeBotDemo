﻿using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Websockets;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex;
using TradeMarket.DataTransfering.Bitmex.Model;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.Model.UserContexts;

namespace TradeMarket.Model.TradeMarkets
{
    public class TradeMarketFactory
    {
        private BitmexWebsocketClient _wsClient;
        private RestfulClient _restClient;
        private IConnectionMultiplexer _multiplexer;

        private IDictionary<string, TradeMarket> _tradeMarkets;

        public TradeMarketFactory(IConnectionMultiplexer multiplexer,BitmexWebsocketClient wsClient,RestfulClient restClient)
        {
            _wsClient = wsClient;
            _restClient = restClient;
            _multiplexer = multiplexer;
            _tradeMarkets = new Dictionary<string, TradeMarket>(new List<KeyValuePair<string, TradeMarket>>());
            _tradeMarkets.Add("bitmex", BuildBitmexTradeMarket());
        }
        #region Concrete TMs Build
        
        public TradeMarket BuildBitmexTradeMarket()
        {
            var wsClient = ClientsFactory.CreateWebsocketClient(BitmexValues.ApiWebsocketTestnetUrl);
            var restClient = ClientsFactory.CreateRestfulClient(BitmexRestufllLink.Testnet);
            var publisherFactory = new BitmexPublisherFactory(_multiplexer,wsClient);
            return new BitmexTradeMarketBuilder()
                .AddCommonClient(_wsClient)
                .AddCommonClient(_restClient)
                .AddPublisherFactory(publisherFactory)
                .AddName("bitmex")

                .Result;
        }

        public TradeMarket SubscribeToLifeLineTopics(BitmexTradeMarket tm,CancellationToken token,ILogger logger)
        {
            return new BitmexTradeMarketBuilder(tm)
                .StartPingPong(token,logger)
                .ReadErrors(token,logger)
                .Result;
        }

        #endregion

        public TradeMarket GetTradeMarket(string name)
        {
            if (_tradeMarkets.ContainsKey(name))
            {
                return _tradeMarkets[name];
            }
            throw new ArgumentException($"{name} hasn't been implemented yet");
        }


       
    }
}
