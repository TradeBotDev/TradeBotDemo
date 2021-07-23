using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Former
{
    public class UserContext
    {
        public string sessionId;
        public string trademarket;
        public string slot;
        public Config configuration;
        public TradeMarketClient tradeMarketClient;
        public Former former;

        public Metadata Meta { get; internal set; }

        public Double AvgPrice { get; set; }

        internal UserContext(TradeMarketClient tradeMarketClient, Former former, string sessionId, string trademarket, string slot)
        {

            Meta.Add("sessionId", sessionId);
            Meta.Add("trademarket", trademarket);
            Meta.Add("slot", slot);

            TradeMarketClient.Configure("https://localhost:5005", 10000);
            tradeMarketClient = TradeMarketClient.GetInstance();
            tradeMarketClient.SetMetadata(meta);
            former = new Former();
            //Конфиг передается как параметр для любого метода
            //former.Config = configuration;

            tradeMarketClient.ObserveOrderBook();
            tradeMarketClient.ObserveBalance();
            tradeMarketClient.ObserveMyOrders();

        }

        public async void FormPurchaseList()
        {
            former.FormPurchaseList(AvgPrice, this);
        }
    }
}
