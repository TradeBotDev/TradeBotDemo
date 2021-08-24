using Bitmex.Client.Websocket;
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

namespace TradeMarket.Model.TradeMarkets
{
    public class TradeMarketFactory
    {
        private BitmexWebsocketClient _wsClient;
        private BitmexRestfulClient _restClient;
        private IConnectionMultiplexer _multiplexer;

        private IDictionary<string, TradeMarket> _tradeMarkets;

        public TradeMarketFactory(IConnectionMultiplexer multiplexer,BitmexWebsocketClient wsClient,BitmexRestfulClient restClient)
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
            var publisherFactory = new BitmexPublisherFactory(_multiplexer);
            return new BitmexTradeMarketBuilder()
                .AddCommonClient(_wsClient)
                .AddCommonClient(_restClient)
                .AddPublisherFactory(publisherFactory)
                .AddName("bitmex")

                .Result;
        }

        public TradeMarket SubscribeToLifeLineTopics(BitmexTradeMarket tm,CancellationToken token)
        {
            return new BitmexTradeMarketBuilder(tm)
                .StartPingPong(token)
                .ReadErrors(token)
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
