using Serilog;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;
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
        private readonly TradeMarketClient _tmClient;

        private readonly ConcurrentDictionary<string, Order> _currentPurchaseOrders;

        private readonly ConcurrentDictionary<string, Order> _myOrders; 

        private double _balanceInRubles = 100;

        private Config config;

        public void SetConfig(Config conf)
        {
            config = conf;
        }

        public Former()
        {
            _tmClient = TradeMarketClient.GetInstance();
            _tmClient.UpdatePurchaseOrders += UpdateCurrentPurchaseOrders;
            _tmClient.UpdateBalance += UpdateCurrentBalance;
            _tmClient.UpdateMyOrders += UpdateMyOrder;
            _tmClient.SellListUpdated += FormSellList;
            
            //кастомный конфиг
            config = new Config
            {
                AvaibleBalance = 1.0,
                ContractValue = 10.0,
                RequiredProfit = 0.5,
                OrderUpdatePriceRange = 1.0,
                SlotFee = 0.2,
                TotalBalance = 0
            };

            _currentPurchaseOrders = new ConcurrentDictionary<string, Order>();
            _myOrders = new ConcurrentDictionary<string, Order>();
        }

        private async void UpdateCurrentPurchaseOrders(Order orderNeededUpdate)
        {
            //Log.Debug("Принял от маркета заказ {id}: цена {price}, количество {value}", orderNeededUpdate.Response.Order.Id, orderNeededUpdate.Response.Order.Price, orderNeededUpdate.Response.Order.Quantity);
            var task = Task.Run(() =>
            {
                //TODO убрать проверку на тип ордера
                if (orderNeededUpdate.Signature.Status == OrderStatus.Open && orderNeededUpdate.Signature.Type == OrderType.Buy)
                    _currentPurchaseOrders.AddOrUpdate(orderNeededUpdate.Id, orderNeededUpdate, (k, v) =>
                    {
                        var price = orderNeededUpdate.Price;
                        if (price != 0) v.Price = price;
                        v.Quantity = orderNeededUpdate.Quantity;
                        v.Signature = orderNeededUpdate.Signature;
                        v.LastUpdateDate = orderNeededUpdate.LastUpdateDate;
                        return v;
                    });
                else if (_currentPurchaseOrders.ContainsKey(orderNeededUpdate.Id)) _currentPurchaseOrders.TryRemove(orderNeededUpdate.Id, out _);
                UpdateFairPrice(_currentPurchaseOrders);
            });
            await task;
        }

        private async void UpdateFairPrice(ConcurrentDictionary<string, Order> currentPurchaseOrders) 
        {
            double fairPrice;
            var task = Task.Run(async () =>
            {
                fairPrice = currentPurchaseOrders.Min(x => x.Value.Price);
                await FitPrice(fairPrice);
            });
            await task;
        }

        private async Task FitPrice(double fairPrice) 
        {
            var ordersToFit = new Dictionary<string, double>();
            foreach (var order in _myOrders)
            {
                if (order.Value.Price + config.OrderUpdatePriceRange < fairPrice)
                {
                    order.Value.Price = fairPrice;
                    _myOrders.TryUpdate(order.Key, order.Value, order.Value);
                    ordersToFit.Add(order.Key, fairPrice);
                }
            }
            await _tmClient.UpdateMyOrdersOnTM(ordersToFit);
        }

        private async void UpdateMyOrder(Order orderNeededUpdate)
        { 
            var task = Task.Run(() =>
            {
                if (orderNeededUpdate.Signature.Status == OrderStatus.Open) _myOrders.TryAdd(orderNeededUpdate.Id, orderNeededUpdate);
                else _myOrders.TryRemove(orderNeededUpdate.Id, out _);
            });
            await task;
        }

        private void UpdateCurrentBalance(Balance balance)
        {
            Log.Debug("Balance updated. New balance: {0}", balance.Value);
            _balanceInRubles = double.Parse(balance.Value);
        }

        public async void FormPurchaseList(double avgPrice)
        {
            Log.Debug("Received from algorithm: {price}", avgPrice);
            double availableBalance = _balanceInRubles * config.AvaibleBalance; //config.AvaibleBalance это доступный баланс в процентах
            var selectedOrders = new Dictionary<string, Order>();
            foreach (var order in _currentPurchaseOrders)
            {
                var price = order.Value.Price;
                if (price + config.RequiredProfit + config.SlotFee < avgPrice)
                    if ((availableBalance - Convert(price)) > 0)
                        selectedOrders.Add(order.Key, order.Value);
            }
            Log.Debug("Formed a list of required orders: {elements}", selectedOrders.ToArray());
            if (selectedOrders.Count != 0) await _tmClient.CloseOrders(selectedOrders, config.ContractValue);
        }

        private async void FormSellList(Dictionary<string, Order> successfulOrders)
        {
            var ordersForSale = new List<SalesOrder>();
            double sellPrice;
            foreach (var order in successfulOrders)
            {
                sellPrice = order.Value.Price + config.RequiredProfit + config.SlotFee;
                ordersForSale.Add(new SalesOrder { price = sellPrice, value = config.ContractValue });
                _myOrders.TryAdd(order.Key,
                    new Order
                    {
                        Id = order.Value.Id,
                        Price = sellPrice,
                        LastUpdateDate = new Google.Protobuf.WellKnownTypes.Timestamp(),
                        Quantity = config.ContractValue,
                        Signature = new OrderSignature { Status = OrderStatus.Open, Type = OrderType.Sell }
                    });
            }
            await _tmClient.PlaceSuccessfulOrders(ordersForSale);
        }

        private double Convert(double priceFromTM)
        {
            return priceFromTM * 0.00000001;
        }
    }
}
