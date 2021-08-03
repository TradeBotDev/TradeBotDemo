using Bitmex.Client.Websocket.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.Model.Publishers;

namespace TradeMarket.Model.TradeMarkets
{
    public interface ITradeMarketBuilder
    {
        public ITradeMarketBuilder AddPublisherFactory(IPublisherFactory factory);

        public ITradeMarketBuilder AddCommonClient(BitmexWebsocketClient client);

        public ITradeMarketBuilder AddCommonClient(BitmexRestfulClient client);

        public void Reset();

        public TradeMarket Result { get; }

    }
}
