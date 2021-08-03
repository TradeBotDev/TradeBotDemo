using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Websockets;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex;
using TradeMarket.DataTransfering.Bitmex.Model;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;

namespace TradeMarket.Model.TradeMarkets
{
    public class TradeMarketFactory
    {
        private IConnectionMultiplexer _multiplexer;

        private IDictionary<string, TradeMarket> _tradeMarkets;

        public TradeMarketFactory(IConnectionMultiplexer multiplexer)
        {
            _multiplexer = multiplexer;
            _tradeMarkets = new Dictionary<string, TradeMarket>(new List<KeyValuePair<string, TradeMarket>>());
            _tradeMarkets.Add("Bitmex", BuildBitmexTradeMarket());
        }
        #region Concrete TMs Build
        
        public TradeMarket BuildBitmexTradeMarket()
        {
            var publisherVactory = new BitmexPublisherFactory();
            var builder = new BitmexTradeMarketBuilder();
            builder.AddCommonClient(new BitmexWebsocketClient(new BitmexWebsocketCommunicator(BitmexValues.ApiWebsocketTestnetUrl)));
            builder.AddCommonClient(new BitmexRestfulClient());
            builder.AddPublisherFactory(publisherVactory);
            return builder.Result;
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


        #region Static Part
        
        #endregion
    }
}
