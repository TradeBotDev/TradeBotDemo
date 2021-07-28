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
        //Класс контекста пользователя
        public string sessionId;
        public string trademarket;
        public string slot;
        public Config configuration;
        private readonly TradeMarketClient _tradeMarketClient;
        private readonly Former _former;

        public Metadata Meta { get; internal set; }

        internal UserContext(string sessionId, string trademarket, string slot)
        {
            this.sessionId = sessionId;
            this.trademarket = trademarket;
            this.slot = slot;


            Meta = new Metadata();
            Meta.Add("sessionid", sessionId);
            Meta.Add("trademarket", trademarket);
            Meta.Add("slot", slot);

            TradeMarketClient.Configure("https://localhost:5005", 10000);

            _tradeMarketClient = new TradeMarketClient();
            _former = new Former(25);
            //Конфиг передается как параметр для любого метода

            _tradeMarketClient.UpdateOrderBook += UpdateOrderBooks;
            _tradeMarketClient.UpdateBalance += UpdateBalance;
            _tradeMarketClient.UpdateMyOrders += UpdateMyOrderList;

            ObserveOrderBook();
            ObserveBalance();
            ObserveMyOrders();
        }

        public async void FormPurchaseOrder()
        {
            await _former.FormPurchaseOrder(this);
        }
        public async void FormSellOrder()
        {
            await _former.FormSellOrder(this);
        }
        private async void UpdateOrderBooks(Order orderNeededUpdate) 
        {
           await _former.UpdateOrderBooks(orderNeededUpdate, this);
        }
        private async void UpdateMyOrderList(Order orderNeededUpdate)
        {
            await _former.UpdateMyOrderList(orderNeededUpdate, this);
        }
        private async void UpdateBalance(int balanceToBuy, int balanceToSell)
        {
            await _former.UpdateBalance(balanceToBuy, balanceToSell);
        }
        public async Task PlaceOrder(double sellPrice, double contractValue)
        {
             await _tradeMarketClient.PlaceOrder(sellPrice, contractValue, this);
        }
        public async Task SetNewPrice(Order orderNeededToUpdate)
        {
            await _tradeMarketClient.SetNewPrice(orderNeededToUpdate, this);
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
