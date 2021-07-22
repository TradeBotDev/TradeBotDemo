using Grpc.Core;
using Serilog;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Former
{
    public class Former
    {
        private readonly TradeMarketClient _tmClient;

        private readonly ConcurrentDictionary<string, Order> _orderBook;

        private readonly ConcurrentDictionary<string, Order> _myOrders;

        private Metadata _meta;

        private double _balance = 100;

        public Config Config
        {
            get; set;
        }

        //TODO формер не должне принимать конфиг как аргмунет конструктора, но должен принимать конфиг ( или контекст пользователя) как аргумент своих методов
        public Former(Config config)
        {
            _tmClient = TradeMarketClient.GetInstance();
            _tmClient.UpdateOrderBook += UpdateOrderBook;
            _tmClient.UpdateBalance += UpdateBalance;
            _tmClient.UpdateMyOrders += UpdateMyOrderList;

            //кастомный конфиг
            //config = new Config
            //{
            //    AvaibleBalance = 1.0,
            //    ContractValue = 10.0,
            //    RequiredProfit = 0.5,
            //    OrderUpdatePriceRange = 1.0,
            //    SlotFee = 0.2,
            //    TotalBalance = 0
            //};
            _config = config;
            _orderBook = new ConcurrentDictionary<string, Order>();
            _myOrders = new ConcurrentDictionary<string, Order>();
        }

        private async void UpdateOrderBook(Order orderNeededUpdate,Config config)
        {
            var task = Task.Run(() =>
            {
                //TODO убрать проверку на тип ордера
                if (orderNeededUpdate.Signature.Status == OrderStatus.Open && orderNeededUpdate.Signature.Type == OrderType.Sell)
                    _orderBook.AddOrUpdate(orderNeededUpdate.Id, orderNeededUpdate, (k, v) =>
                    {
                        var price = orderNeededUpdate.Price;
                        if (price != 0) v.Price = price;
                        v.Quantity = orderNeededUpdate.Quantity;
                        v.Signature = orderNeededUpdate.Signature;
                        v.LastUpdateDate = orderNeededUpdate.LastUpdateDate;
                        return v;
                    });
                else if (_orderBook.ContainsKey(orderNeededUpdate.Id)) _orderBook.TryRemove(orderNeededUpdate.Id, out _);

            });
            await task;
            //???????????????????????????????????????????????????????????????????????????????????????????
            if (_orderBook.Count > 2) await FitPrices(_orderBook);
        }

        private async void SellPartOfContracts(string id, Order orderNeededUpdate, double sellPrice) 
        {
            double newQuantity;
            if (_myOrders.TryGetValue(id, out Order beforeUpdate))
                if ((newQuantity = beforeUpdate.Quantity - orderNeededUpdate.Quantity) > 0)
                    await _tmClient.PlaceSellOrder(sellPrice, newQuantity);
        }
        //необходимо ревью
        private async void UpdateMyOrderList(Order orderNeededUpdate)
        {
            var id = orderNeededUpdate.Id;
            var status = orderNeededUpdate.Signature.Status;
            var type = orderNeededUpdate.Signature.Type;
            var sellPrice = orderNeededUpdate.Price + config.RequiredProfit + config.SlotFee;
            var task = Task.Run(async () =>
            {
                if (status == OrderStatus.Open && type == OrderType.Buy)
                {
                    SellPartOfContracts(id, orderNeededUpdate, sellPrice);
                    _myOrders.AddOrUpdate(id, orderNeededUpdate, (k, v) =>
                    {
                        v.Price = orderNeededUpdate.Price;
                        v.Quantity = orderNeededUpdate.Quantity;
                        return v;
                    });
                }
                if (status == OrderStatus.Open && type == OrderType.Sell)
                    _myOrders.AddOrUpdate(id, orderNeededUpdate, (k, v) =>
                    {
                        v.Price = orderNeededUpdate.Price;
                        v.Quantity = orderNeededUpdate.Quantity;
                        return v;
                    });
                if (status == OrderStatus.Closed && type == OrderType.Buy)
                {
                    await _tmClient.PlaceSellOrder(sellPrice, orderNeededUpdate.Quantity);
                    _myOrders.TryRemove(id, out _);
                }
                if (status == OrderStatus.Closed && type == OrderType.Sell)
                    _myOrders.TryRemove(id, out _);
            });
            await task;
        }

        private void UpdateBalance(Balance balance)
        {
            Log.Debug("Balance updated. New balance: {0}", balance.Value);
            _balance = double.Parse(balance.Value);
        }
        //необходимо ревью 
        private async Task FitPrices(ConcurrentDictionary<string, Order> currentPurchaseOrdersForFairPrice)
        {
            double fairPrice = 0;
            var ordersNeededToFit = new Dictionary<string, double>();
            var calcFairPrice = Task.Run(() => fairPrice = currentPurchaseOrdersForFairPrice.Min(x => x.Value.Price));
            var checkPrices = Task.Run(() =>
            {
                foreach (var order in _myOrders)
                {
                    if (order.Value.Price + config.OrderUpdatePriceRange < fairPrice)
                    {
                        order.Value.Price = fairPrice;
                        _myOrders.TryUpdate(order.Key, order.Value, order.Value);
                        ordersNeededToFit.Add(order.Key, fairPrice);
                    }
                }
            });
            await calcFairPrice;
            await checkPrices;

            if (ordersNeededToFit.Count != 0)
                await _tmClient.TellTMUpdateMyOreders(ordersNeededToFit);
        }

        public async void FormPurchaseList(double avgPrice,UserContext context)
        {
            Log.Debug("Received from algorithm: {price}", avgPrice);
            //config.AvaibleBalance это доступный баланс в процентах
            double availableBalance = _balance * context.config.AvaibleBalance;
            var selectedOrders = new Dictionary<double, double>();
            foreach (var order in _orderBook)
            {
                if (IsPriceCorrect(order.Value.Price, avgPrice, availableBalance))
                    selectedOrders.Add(order.Value.Price, config.ContractValue);
            }
            Log.Debug("Formed a list of required orders: {elements}", selectedOrders.Count);
            if (selectedOrders.Count != 0)
                await _tmClient.PlacePurchaseOrders(selectedOrders);
        }

        private bool IsPriceCorrect(double estimatedPrice, double cuttingPrice, double availableBalance)
        {
            if (estimatedPrice < cuttingPrice && (availableBalance - Convert(estimatedPrice) * config.ContractValue) > 0) return true;
            else return false;

        }

        private double Convert(double priceFromTM)
        {
            return priceFromTM * 0.00000001;
        }
    }
}
