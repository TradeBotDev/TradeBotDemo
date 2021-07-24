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

        internal UserContext(string sessionId, string trademarket, string slot)
        {

            Meta.Add("sessionId", sessionId);
            Meta.Add("trademarket", trademarket);
            Meta.Add("slot", slot);

            TradeMarketClient.Configure("https://localhost:5005", 10000);

            tradeMarketClient = new TradeMarketClient();
            tradeMarketClient.SetMetadata(Meta);

            former = new Former();
            //Конфиг передается как параметр для любого метода

            tradeMarketClient.UpdateOrderBook += UpdateOrderBook;
            tradeMarketClient.UpdateBalance += UpdateBalance;
            tradeMarketClient.UpdateMyOrders += UpdateMyOrderList;

        }

        public async void FormPurchaseList(double AvgPrice)
        {
            former.FormPurchaseList(AvgPrice, this);
        }
        private async void UpdateOrderBook(Order orderNeededUpdate) 
        {
            former.UpdateOrderBook(orderNeededUpdate, this);
        }
        private async void UpdateMyOrderList(Order orderNeededUpdate)
        {
            former.UpdateMyOrderList(orderNeededUpdate, this);
        }
        private async void UpdateBalance(Balance balance)
        {
            former.UpdateBalance(balance);
        }
    }
}
