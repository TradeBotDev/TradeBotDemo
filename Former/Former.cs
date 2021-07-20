using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using SubscribeOrdersResponse = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersResponse;

namespace Former
{
    public class SalesOrder 
    {
        public double price;
        public double value;
    }

    public class Former
    {
        private readonly int _ordersCount;

        private readonly TradeMarketClient _tmClient;

        private readonly ConcurrentDictionary<string, SubscribeOrdersResponse> _currentPurchaseOrders;

        private List<SalesOrder> _ordersForSale;

        private readonly List<Order> _myOrders;

        private double bal1;

        private double bal2;

        private Config config;

        public void SetConfig(Config conf)
        {
            config = conf;
        }

        public Former(int ordersToSave)
        {
            _ordersCount = ordersToSave;
            _tmClient = TradeMarketClient.GetInstance();
            _tmClient.UpdatePurchaseOrders += UpdateCurrentPurchaseOrders;
            _tmClient.SellSuccessOrders += SellPurchasedOrders;
            _tmClient.UpdateBalance += UpdateCurrentBalance;

            _currentPurchaseOrders = new ConcurrentDictionary<string, SubscribeOrdersResponse>();
            _ordersForSale = new List<SalesOrder>();
        }

        private async void UpdateCurrentPurchaseOrders(SubscribeOrdersResponse orderNeededUpdate)
        {
            //Log.Debug("Принял от маркета заказ {id}: цена {price}, количество {value}", orderNeededUpdate.Response.Order.Id, orderNeededUpdate.Response.Order.Price, orderNeededUpdate.Response.Order.Quantity);
            var task = Task.Run(() =>
            {
                if (orderNeededUpdate.Response.Order.Signature.Status == OrderStatus.Open)
                    _currentPurchaseOrders.AddOrUpdate(orderNeededUpdate.Response.Order.Id, orderNeededUpdate, (k, v) => {
                        //todo убедиться что цена приходит чистым нулем и не нужно проверять по эпсилону
                        var price = orderNeededUpdate.Response.Order.Price;
                        if (price != 0) v.Response.Order.Price = price;
                        v.Response.Order.Quantity = orderNeededUpdate.Response.Order.Quantity;
                        v.Response.Order.Signature = orderNeededUpdate.Response.Order.Signature;
                        v.Response.Order.LastUpdateDate = orderNeededUpdate.Response.Order.LastUpdateDate;
                        return v;
                    });
                else if (_currentPurchaseOrders.ContainsKey(orderNeededUpdate.Response.Order.Id)) _currentPurchaseOrders.TryRemove(orderNeededUpdate.Response.Order.Id, out _);
            });
            await task;
        }

        private void SellPurchasedOrders(Dictionary<string, SubscribeOrdersResponse> successfulOrders)
        {
            _ordersForSale.Clear();
            double sellPrice;
            foreach (var order in successfulOrders)
            {
                sellPrice = order.Value.Response.Order.Price + config.RequiredProfit + config.SlotFee;
                _ordersForSale.Add(new SalesOrder { price = sellPrice, value = config.ContractValue });
                _myOrders.Add(new Order
                {
                    Id = order.Value.Response.Order.Id,
                    Price = sellPrice,
                    LastUpdateDate = new Google.Protobuf.WellKnownTypes.Timestamp(),
                    Quantity = config.ContractValue,
                    Signature = new OrderSignature { Status = OrderStatus.Open, Type = OrderType.Sell }
                });
            }
            _tmClient.PlaceSuccessfulOrders(_ordersForSale);
        }

        public void UpdateCurrentBalance(Balance balance)
        {
            bal1 = double.Parse(balance.bal1);
            bal2 = double.Parse(balance.bal2);
        }

        private double Convert(double priceFromTM) 
        {
            return priceFromTM * 0.00000001;
        }

        public async void FormShoppingList(double avgPrice)
        {
            Log.Debug("Получено от алгоритма: {price}", avgPrice);

            var selectedOrders = _currentPurchaseOrders.Where(order => order.Value.Response.Order.Price <= avgPrice).ToDictionary(x => x.Key, x => x.Value);

            Log.Debug("Сформировал список необходимых ордеров: {elements}", selectedOrders.ToArray());
            await _tmClient.CloseOrders(selectedOrders);
        }
    }
}
