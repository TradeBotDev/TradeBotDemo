using Serilog;
using System;
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

        private readonly List<SubscribeOrdersResponse> _currentPurchaseOrders;

        private List<SubscribeOrdersResponse> _successfulOrders;

        private List<SalesOrder> _ordersForSale;

        private readonly List<Order> _myOrders;

        private double bal1;

        private double bal2;

        private Config config;

        public Former(int ordersCount)
        {
            _ordersCount = ordersCount;
            _tmClient = TradeMarketClient.GetInstance();
            _tmClient.UpdatePurchaseOrders += UpdateCurrentPurchaseOrders;
            _tmClient.SellSuccessOrders += UpdateSuccessfullyPurchasedOrders;
            _tmClient.UpdateBalance += UpdateCurrentBalance;

            _currentPurchaseOrders = new List<SubscribeOrdersResponse>();
            _successfulOrders = new List<SubscribeOrdersResponse>();
            _ordersForSale = new List<SalesOrder>();
        }

        private async void UpdateCurrentPurchaseOrders(SubscribeOrdersResponse orderNeededUpdate)
        {
            Log.Debug("Принял от маркета заказ {id}: цена {price}, количество {value}", orderNeededUpdate.Response.Order.Id, orderNeededUpdate.Response.Order.Price, orderNeededUpdate.Response.Order.Quantity);

            var task = Task.Run(() =>
            {
                int index = _currentPurchaseOrders.FindIndex(x => x.Response.Order.Id == orderNeededUpdate.Response.Order.Id);
                if (orderNeededUpdate.Response.Order.Signature.Status == OrderStatus.Open)
                    if (_currentPurchaseOrders.Exists(x => x.Response.Order.Id == orderNeededUpdate.Response.Order.Id))
                        _currentPurchaseOrders[index] = orderNeededUpdate;
                    else _currentPurchaseOrders.Add(orderNeededUpdate);
                else _currentPurchaseOrders.RemoveAt(index);

                _currentPurchaseOrders.Sort(new ReplyComparator());
            });
            await task;
        }

        private void UpdateSuccessfullyPurchasedOrders(List<SubscribeOrdersResponse> successfulOrders)
        {
            _successfulOrders = successfulOrders.ToList();
            _ordersForSale.Clear();
            double sellPrice;
            foreach (var order in _successfulOrders)
            {
                sellPrice = order.Response.Order.Price + config.RequiredProfit + config.SlotFee;
                _ordersForSale.Add(new SalesOrder { price = sellPrice, value = config.ContractValue });
                _myOrders.Add(new Order 
                { 
                    Id = order.Response.Order.Id, 
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

        public async void FormShoppingList(double avgPrice)
        {
            Log.Debug("Получено от алгоритма: {price}", avgPrice);

            var selectedOrders = _currentPurchaseOrders.Where(order => order.Response.Order.Price <= avgPrice).ToList();

            Log.Debug("Сформировал список необходимых ордеров: {elements}", selectedOrders.ToArray());
            await _tmClient.SendShoppingList(selectedOrders);
        }
    }
}
