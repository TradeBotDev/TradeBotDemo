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
        public string trademarketName;
        public string slotName;
        public Config configuration;
        private TradeMarketClient _tradeMarketClient;
        private Former _former;

        public Metadata Meta { get; internal set; }

        internal UserContext(string sessionId, string trademarket, string slot)
        {

            Meta.Add("sessionId", sessionId);
            Meta.Add("trademarket", trademarket);
            Meta.Add("slot", slot);

            TradeMarketClient.Configure("https://localhost:5005", 10000);

            _tradeMarketClient = new TradeMarketClient();
            _former = new Former();
            //Конфиг передается как параметр для любого метода

            _tradeMarketClient.UpdateOrderBook += UpdateOrderBook;
            _tradeMarketClient.UpdateBalance += UpdateBalance;
            _tradeMarketClient.UpdateMyOrders += UpdateMyOrderList;

            ObserveOrderBook();
            ObserveBalance();
            ObserveMyOrders();
        }

        public async void FormPurchaseList(double AvgPrice)
        {
            await _former.FormPurchaseList(AvgPrice, this);
        }
        private async void UpdateOrderBook(Order orderNeededUpdate) 
        {
           await _former.UpdateOrderBook(orderNeededUpdate, this);
        }
        private async void UpdateMyOrderList(Order orderNeededUpdate)
        {
            await _former.UpdateMyOrderList(orderNeededUpdate, this);
        }
        private async void UpdateBalance(Balance balance)
        {
            await _former.UpdateBalance(balance);
        }
        public async Task PlaceOrder(double sellPrice, double contractValue)
        {
             await _tradeMarketClient.PlaceOrder(sellPrice, contractValue, this);
        }
        public async Task PlaceOrdersList(Dictionary<double, double> purchaseList) 
        {
             await _tradeMarketClient.PlaceOrdersList(purchaseList, this);
        }
        public async Task TellTMUpdateMyOrders(Dictionary<string, double> orderToUpdate)
        {
            await _tradeMarketClient.TellTMUpdateMyOrders(orderToUpdate, this);
        }
        private async void ObserveOrderBook()
        {
            await _tradeMarketClient.ObserveOrderBook(this);
        }
        private async void ObserveBalance()
        {
            await _tradeMarketClient.ObserveBalance(this);
        }
        private async void ObserveMyOrders()
        {
            await _tradeMarketClient.ObserveMyOrders(this);
        }
    }
}
