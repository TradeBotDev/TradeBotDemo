using Serilog;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        private readonly ConcurrentDictionary<string, SubscribeOrdersResponse> _currentPurchaseOrders;

        private readonly ConcurrentDictionary<string, Order> _myOrders;

        private double _balanceInRubles;

        //private double bal2;

        private Config config;

        public void SetConfig(Config conf)
        {
            config = conf;
        }

        public Former()
        {
            _tmClient = TradeMarketClient.GetInstance();
            _tmClient.UpdatePurchaseOrders += UpdateCurrentPurchaseOrders;
            _tmClient.SellSuccessOrders += SellPurchasedOrders;
            _tmClient.UpdateBalance += UpdateCurrentBalance;
            _tmClient.UpdateMyOrders += UpdateMyOrder;

            //кастомный конфиг
            config = new Config
            {
                AvaibleBalance = 1.0,
                ContractValue = 10,
                RequiredProfit = 0.5,
                OrderUpdatePriceRange = 1.0,
                SlotFee = 0.2,
                TotalBalance = 0
            };

            _currentPurchaseOrders = new ConcurrentDictionary<string, SubscribeOrdersResponse>();
            _myOrders = new ConcurrentDictionary<string, Order>();
        }

        private async void UpdateCurrentPurchaseOrders(SubscribeOrdersResponse orderNeededUpdate)
        {
            //Log.Debug("Принял от маркета заказ {id}: цена {price}, количество {value}", orderNeededUpdate.Response.Order.Id, orderNeededUpdate.Response.Order.Price, orderNeededUpdate.Response.Order.Quantity);
            var task = Task.Run(() =>
            {
                if (orderNeededUpdate.Response.Order.Signature.Status == OrderStatus.Open)
                    _currentPurchaseOrders.AddOrUpdate(orderNeededUpdate.Response.Order.Id, orderNeededUpdate, (k, v) =>
                    {
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
            var _ordersForSale = new List<SalesOrder>();
            double sellPrice;
            foreach (var order in successfulOrders)
            {
                sellPrice = order.Value.Response.Order.Price + config.RequiredProfit + config.SlotFee;
                _ordersForSale.Add(new SalesOrder { price = sellPrice, value = config.ContractValue });
                _myOrders.TryAdd(order.Value.Response.Order.Id,
                    new Order
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

        private void UpdateCurrentBalance(Balance balance)
        {
            Log.Debug("Balance updated. New balance: {0}", balance.bal1);
            _balanceInRubles = double.Parse(balance.bal1);
            //bal2 = double.Parse(balance.bal2);
        }

        private async void UpdateMyOrder(SubscribyMyOrdersResponse orderNeededUpdate)
        {
            string key;
            var task = Task.Run(() =>
            {
                key = orderNeededUpdate.Changed.Id;
                if (orderNeededUpdate.Changed.Signature.Status == OrderStatus.Open)
                    _myOrders.TryUpdate(key, orderNeededUpdate.Changed, _myOrders[key]);
                else _myOrders.TryRemove(key, out _);
            });
            await task;
        }

        private static double Convert(double priceFromTM)
        {
            return priceFromTM * 0.00000001;
        }

        public async void FormShoppingList(double avgPrice)
        {
            double availableBalance = _balanceInRubles * config.AvaibleBalance;//  config.AvaibleBalance - доступный баланс в процентах
            Log.Debug("Received from algorithm: {price}", avgPrice);
            var selectedOrders = new Dictionary<string, SubscribeOrdersResponse>();

            foreach (var order in _currentPurchaseOrders)
            {
                var price = order.Value.Response.Order.Price;
                if (price <= avgPrice)
                    if ((availableBalance - Convert(price)) > 0)
                        selectedOrders.Add(order.Key, order.Value);
            }
            Log.Debug("Formed a list of required orders: {elements}", selectedOrders.ToArray());
            await _tmClient.CloseOrders(selectedOrders, config.ContractValue);
        }
    }
}
