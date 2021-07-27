﻿using Serilog;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Former
{
    public class Former
    {
        private readonly ConcurrentDictionary<string, Order> _purchaseOrderBook;

        private readonly ConcurrentDictionary<string, Order> _sellOrderBook;

        private readonly ConcurrentDictionary<string, Order> _myOrders;

        private double _balance = 100;

        //TODO формер не должне принимать конфиг как аргмунет конструктора, но должен принимать конфиг ( или контекст пользователя) как аргумент своих методов
        public Former()
        {
            _sellOrderBook = new ConcurrentDictionary<string, Order>();
            _purchaseOrderBook = new ConcurrentDictionary<string, Order>();

            _myOrders = new ConcurrentDictionary<string, Order>();
        }

        private async Task UpdateConcreteBook(ConcurrentDictionary<string, Order> bookNeededUpdate, Order order, UserContext context)
        {
            var task = Task.Run(async () =>
            {
                if (order.Signature.Status == OrderStatus.Open)
                    bookNeededUpdate.AddOrUpdate(order.Id, order, (k, v) =>
                    {
                        var price = order.Price;
                        if (price != 0) v.Price = price;
                        v.Quantity = order.Quantity;
                        v.Signature = order.Signature;
                        v.LastUpdateDate = order.LastUpdateDate;
                        return v;
                    });
                else if (bookNeededUpdate.ContainsKey(order.Id))
                    bookNeededUpdate.TryRemove(order.Id, out _);
                await FitPrices(bookNeededUpdate, context);
            });
            await task;
        }

        public async Task UpdateOrderBooks(Order orderNeededUpdate, UserContext context)
        {
            var task = Task.Run(async () =>
            {
                if (orderNeededUpdate.Signature.Type == OrderType.Sell) await UpdateConcreteBook(_purchaseOrderBook, orderNeededUpdate, context);
                if (orderNeededUpdate.Signature.Type == OrderType.Buy) await UpdateConcreteBook(_sellOrderBook, orderNeededUpdate, context);
            });
            await task;
        }

        public Task UpdateBalance(Balance balance)
        {
            Log.Information("Balance updated. New balance: {0}", balance.Value);
            _balance = double.Parse(balance.Value);
            return Task.CompletedTask;
        }

        public async Task UpdateMyOrderList(Order orderNeededUpdate, UserContext context)
        {
            var id = orderNeededUpdate.Id;
            var status = orderNeededUpdate.Signature.Status;
            var type = orderNeededUpdate.Signature.Type;
            var task = Task.Run(() =>
            {
                if (status == OrderStatus.Open && type == OrderType.Buy)
                {
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

        private async Task SellPartOfContracts(string id, Order orderNeededUpdate, double sellPrice, UserContext context)
        {
            double newQuantity;
            if (_myOrders.TryGetValue(id, out Order beforeUpdate))
                if ((newQuantity = beforeUpdate.Quantity - orderNeededUpdate.Quantity) > 0)
                    await context.PlaceOrder(sellPrice, -newQuantity);
        }

        private async Task FitPrices(ConcurrentDictionary<string, Order> ordersForFairPrice, UserContext context)
        {
            //????????????????????????????????????????????????//
            double fairPrice = 0;
            var ordersNeededToFit = new Dictionary<string, double>();
            var calcFairPrice = Task.Run(() => fairPrice = ordersForFairPrice.Min(x => x.Value.Price));
            await calcFairPrice;
            var checkPrices = Task.Run(async () =>
            {
                foreach (var order in _myOrders)
                {
                    if (order.Value.Price < fairPrice)
                    {
                        order.Value.Price = fairPrice;
                        _myOrders.TryUpdate(order.Key, order.Value, order.Value);

                        await context.SetNewPrice(order.Value);
                    }
                }
            });
            await checkPrices;  
        }

        public async Task FormPurchaseOrder(UserContext context)
        {
            Log.Debug("Playing long...");
            double availableBalance = _balance * context.configuration.AvaibleBalance;
            //Calc pruchase price

            double purchaseFairPrice = _purchaseOrderBook.Min(x => x.Value.Price);
            double sellPrice = purchaseFairPrice + purchaseFairPrice * context.configuration.RequiredProfit;
            double quantity = 0;


            while (quantity / purchaseFairPrice < availableBalance)
                quantity += 100;
            //buy order at fair price
            await context.PlaceOrder(purchaseFairPrice, quantity);
            //sell order at price with required profit
            await context.PlaceOrder(sellPrice, -quantity);
        }

        public async Task FormSellOrder(UserContext context)
        {
            Log.Debug("Playing short...");
            double availableBalance = _balance * context.configuration.AvaibleBalance;
            //Calc pruchase price
            double sellFairPrice = _sellOrderBook.Min(x => x.Value.Price);

            double sellPrice = sellFairPrice - sellFairPrice * context.configuration.RequiredProfit;
            double quantity = 0;


            while (quantity / sellFairPrice < availableBalance)
                quantity += 100;

            //sell order at fair price
            await context.PlaceOrder(sellFairPrice, -quantity);
            //buy order at price with required profit
            await context.PlaceOrder(sellPrice, quantity);
        }
    }
}
