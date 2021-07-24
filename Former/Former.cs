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
        private readonly ConcurrentDictionary<string, Order> _orderBook;

        private readonly ConcurrentDictionary<string, Order> _myOrders;

        private double _balance = 100;

        //TODO формер не должне принимать конфиг как аргмунет конструктора, но должен принимать конфиг ( или контекст пользователя) как аргумент своих методов
        public Former()
        {
            _orderBook = new ConcurrentDictionary<string, Order>();
            _myOrders = new ConcurrentDictionary<string, Order>();
        }

        public async void UpdateOrderBook(Order orderNeededUpdate, UserContext context)
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
            if (_orderBook.Count > 2) await FitPrices(_orderBook, context.configuration.OrderUpdatePriceRange, context);
        }

        private async void SellPartOfContracts(string id, Order orderNeededUpdate, double sellPrice, UserContext context) 
        {
            double newQuantity;
            if (_myOrders.TryGetValue(id, out Order beforeUpdate))
                if ((newQuantity = beforeUpdate.Quantity - orderNeededUpdate.Quantity) > 0)
                    await UserContextFactory.GetUserContext(context.sessionId, context.trademarket, context.slot).tradeMarketClient.PlaceSellOrder(sellPrice, newQuantity);
        }
        //необходимо ревью
        public async void UpdateMyOrderList(Order orderNeededUpdate, UserContext context)
        {
            var id = orderNeededUpdate.Id;
            var status = orderNeededUpdate.Signature.Status;
            var type = orderNeededUpdate.Signature.Type;
            var sellPrice = orderNeededUpdate.Price + context.configuration.RequiredProfit + context.configuration.SlotFee;
            var task = Task.Run(async () =>
            {
                if (status == OrderStatus.Open && type == OrderType.Buy)
                {
                    SellPartOfContracts(id, orderNeededUpdate, sellPrice, context);
                    _myOrders.AddOrUpdate(id, orderNeededUpdate, (k, v) =>
                    {
                        Log.Information("Order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} added or updated", id, orderNeededUpdate.Price, orderNeededUpdate.Quantity, orderNeededUpdate.Signature.Type, orderNeededUpdate.Signature.Status);
                        v.Price = orderNeededUpdate.Price;
                        v.Quantity = orderNeededUpdate.Quantity;
                        return v;
                    });
                }
                if (status == OrderStatus.Open && type == OrderType.Sell)
                    _myOrders.AddOrUpdate(id, orderNeededUpdate, (k, v) =>
                    {
                        Log.Information("Order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} added or updated", id, orderNeededUpdate.Price, orderNeededUpdate.Quantity, orderNeededUpdate.Signature.Type, orderNeededUpdate.Signature.Status);
                        v.Price = orderNeededUpdate.Price;
                        v.Quantity = orderNeededUpdate.Quantity;
                        return v;
                    });
                if (status == OrderStatus.Closed && type == OrderType.Buy)
                {
                    Log.Information("Order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} removed", id, orderNeededUpdate.Price, orderNeededUpdate.Quantity, orderNeededUpdate.Signature.Type, orderNeededUpdate.Signature.Status);
                    await UserContextFactory.GetUserContext(context.sessionId, context.trademarket, context.slot).tradeMarketClient.PlaceSellOrder(sellPrice, orderNeededUpdate.Quantity);
                    _myOrders.TryRemove(id, out _);
                }
                if (status == OrderStatus.Closed && type == OrderType.Sell)
                {
                    Log.Information("Order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} removed", id, orderNeededUpdate.Price, orderNeededUpdate.Quantity, orderNeededUpdate.Signature.Type, orderNeededUpdate.Signature.Status);
                    _myOrders.TryRemove(id, out _);
                }
            });
            await task;
        }

        public void UpdateBalance(Balance balance)
        {
            Log.Debug("Balance updated. New balance: {0}", balance.Value);
            _balance = double.Parse(balance.Value);
        }
        //необходимо ревью 
        private async Task FitPrices(ConcurrentDictionary<string, Order> currentPurchaseOrdersForFairPrice, double orderUpdateRange, UserContext context)
        {
            double fairPrice = 0;
            var ordersNeededToFit = new Dictionary<string, double>();
            var calcFairPrice = Task.Run(() => fairPrice = currentPurchaseOrdersForFairPrice.Min(x => x.Value.Price));
            var checkPrices = Task.Run(() =>
            {
                foreach (var order in _myOrders)
                {
                    if (order.Value.Price + orderUpdateRange < fairPrice)
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
                await UserContextFactory.GetUserContext(context.sessionId, context.trademarket, context.slot).tradeMarketClient.TellTMUpdateMyOreders(ordersNeededToFit);
        }

        public async void FormPurchaseList(double avgPrice, UserContext context)
        {
            Log.Debug("Received from algorithm: {price}", avgPrice);
            //config.AvaibleBalance это доступный баланс в процентах
            double availableBalance = _balance * context.configuration.AvaibleBalance;
            var selectedOrders = new Dictionary<double, double>();
            foreach (var order in _orderBook)
            {
                if (IsPriceCorrect(order.Value.Price, avgPrice, availableBalance, context.configuration.ContractValue))
                    selectedOrders.Add(order.Value.Price, context.configuration.ContractValue);
            }
            Log.Debug("Formed a list of required orders:");
            foreach (var order in selectedOrders)
            {
                Log.Debug("Price: {1} Quantity: {2}", order.Key, order.Value);
            }
            if (selectedOrders.Count != 0)
                await UserContextFactory.GetUserContext(context.sessionId, context.trademarket, context.slot).tradeMarketClient.PlacePurchaseOrders(selectedOrders);
        }

        private bool IsPriceCorrect(double estimatedPrice, double cuttingPrice, double availableBalance, double contractValue)
        {
            if (estimatedPrice < cuttingPrice && (availableBalance - Convert(estimatedPrice) * contractValue) > 0) return true;
            else return false;
        }

        private double Convert(double priceFromTM)
        {
            return priceFromTM * 0.00000001;
        }
    }
}
