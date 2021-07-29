using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;

namespace Former
{
    public class Former
    {
        private readonly ConcurrentDictionary<string, Order> _purchaseOrderBook;

        private readonly ConcurrentDictionary<string, Order> _sellOrderBook;

        private readonly ConcurrentDictionary<string, Order> _myOrders;

        private int _totalBalance;

        private int _availableBalance;

        private int _positionSize;

        private int _bookSize;

        enum OrderBookType 
        {
            Sell,
            Buy
        }

        public Former(int bookSize)
        {
            _sellOrderBook = new ConcurrentDictionary<string, Order>();
            _purchaseOrderBook = new ConcurrentDictionary<string, Order>();
            _myOrders = new ConcurrentDictionary<string, Order>();
            _bookSize = bookSize;
        }

        /// <summary>
        /// апдейтит конкретную книгу ордеров (на покупку или продажу)
        /// </summary>
        private async Task UpdateConcreteBook(ConcurrentDictionary<string, Order> bookNeededUpdate, Order order, UserContext context)
        {
            if (CheckContext(context)) return;
            var task = Task.Run(() =>
            {
                //если ордер имеет статус открытый, то он добавляется, либо апдейтится, если закрытый, то удаляется из книги
                if (order.Signature.Status == OrderStatus.Open)
                    bookNeededUpdate.AddOrUpdate(order.Id, order, (k, v) =>
                    {
                        if (order.Price != 0) v.Price = order.Price;
                        if (order.Quantity != 0) v.Quantity = order.Quantity;
                        if (order.Signature.Type != OrderType.Unspecified) v.Signature = order.Signature;
                        v.LastUpdateDate = order.LastUpdateDate;
                        return v;
                    });
                else if (bookNeededUpdate.ContainsKey(order.Id))
                    bookNeededUpdate.TryRemove(order.Id, out _);
            });
            await task;
        }

        /// <summary>
        /// Обновляет одну из книг ордеров с вновь пришедшим ордером
        /// </summary>
        public async Task UpdateOrderBooks(Order newComingOrder, UserContext context)
        {
            if (CheckContext(context)) return;
            var task = Task.Run(async () =>
            {
                //выбирает, какую книгу апдейтить
                if (newComingOrder.Signature.Type == OrderType.Buy)
                {
                    await UpdateConcreteBook(_purchaseOrderBook, newComingOrder, context);
                    //если книга ордеров полностью заполнилась, вычисление рыночной цены, выполняющейся в методе FitPrices, будет корректным
                    if (_purchaseOrderBook.Count >= _bookSize) await FitPrices(_purchaseOrderBook, OrderBookType.Buy, context);
                }
                if (newComingOrder.Signature.Type == OrderType.Sell)
                {
                    await UpdateConcreteBook(_sellOrderBook, newComingOrder, context);
                    //если книга ордеров полностью заполнилась, вычисление рыночной цены, выполняющейся в методе FitPrices, будет корректным
                    if (_purchaseOrderBook.Count >= _bookSize) await FitPrices(_sellOrderBook, OrderBookType.Sell, context);
                }
            });
            await task;
        }
        
        /// <summary>
        /// подгоняет мои ордера под рыночную цену (и на покупку и на продажу)
        /// </summary>
        private async Task FitPrices(ConcurrentDictionary<string, Order> ordersForFairPrice, OrderBookType bookType, UserContext context)
        {
            if (CheckContext(context)) return;
            var checkPrices = Task.Run(async () =>
            {
                //в зависимости от того, какая книга обновлялась, вычисляется рыночная цена
                if (bookType == OrderBookType.Sell)
                {
                    double fairPrice = ordersForFairPrice.Min(x => x.Value.Price);
                    foreach (var order in _myOrders)
                    {
                        //проверяем все свои ордера, необходимо ли им подогнаться по рыночную цену
                        if (order.Value.Price + context.configuration.OrderUpdatePriceRange > fairPrice )
                        {
                            if (!_myOrders.TryGetValue(order.Key, out _)) return;
                            //отправляется запрос в тм, чтобы он перевыставил ордер (поменял цену ордера) на новую (рыночную)
                            AmmendOrderResponse response = await context.SetNewPrice(order.Value);
                            if (response.Response.Code == ReplyCode.Succeed) 
                            {
                                var temp = order.Value;
                                order.Value.Price = fairPrice;
                                _myOrders.TryUpdate(order.Key, order.Value, temp);
                            }
                            Log.Information("Order {0} amended with {1} {2} {3}", order.Key, fairPrice, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Failure ? response.Response.Message : "");
                        }
                    }
                    return;
                }
                if (bookType == OrderBookType.Buy)
                {
                    double fairPrice = ordersForFairPrice.Max(x => x.Value.Price);
                    foreach (var order in _myOrders)
                    {
                        if (order.Value.Price + context.configuration.OrderUpdatePriceRange < fairPrice)
                        {
                            if (!_myOrders.TryGetValue(order.Key, out _)) return;
                            AmmendOrderResponse response = await context.SetNewPrice(order.Value);
                            if (response.Response.Code == ReplyCode.Succeed)
                            {
                                var temp = order.Value;
                                order.Value.Price = fairPrice;
                                _myOrders.TryUpdate(order.Key, order.Value, temp);
                            }
                            Log.Information("Order {0} amended with {1} {2} {3}", order.Key, fairPrice, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Failure ? response.Response.Message : "");
                        }
                    }
                    return;
                }
            });
            await checkPrices;
        }

        internal Task UpdatePosition(double currentQuantity)
        {
            if (_positionSize != currentQuantity) Log.Information("Current postition: {0}", _positionSize);
            _positionSize = (int)currentQuantity;
            return Task.CompletedTask;
        }

        /// <summary>
        /// обновляет балан для формирования верных списков 
        /// </summary>
        internal Task UpdateBalance(int availableBalance, int totalBalance)
        {
            if (_availableBalance != availableBalance || _totalBalance != totalBalance) Log.Information("Balance updated. Available balance: {0}, Total balance: {1}", availableBalance, totalBalance);
            _availableBalance = availableBalance;
            _totalBalance = totalBalance;
            return Task.CompletedTask;
        }

        /// <summary>
        /// обновляет список моих ордеров по подписке
        /// </summary>
        internal async Task UpdateMyOrderList(Order newComingOrder, UserContext context)
        {
            if (CheckContext(context)) return;
            Order oldOrder;
            var id = newComingOrder.Id;
            var price = newComingOrder.Price;
            var quantity = newComingOrder.Quantity;
            var type = newComingOrder.Signature.Type;
            var status = newComingOrder.Signature.Status;

            //пытается получить ордер из списка, то просто выходим из метода, т.к. нам нельзя его добавлять здесь для предотвращения цикла перевыставления ордеров
            if (_myOrders.TryGetValue(id, out oldOrder))
            {
                //так как ордер сущетсвует в нашем списке, вероятно он закрыт почти или полностью и мы будем его продвать, поэтому сразу вычисляем цену для продажи
                double sellPrice = oldOrder.Price + oldOrder.Price * context.configuration.RequiredProfit;
                double newQuantity;
                if (status == OrderStatus.Closed && oldOrder.Signature.Type == OrderType.Buy)
                {
                    PlaceOrderResponse response = await context.PlaceOrder(sellPrice, -oldOrder.Quantity);
                    if (response.Response.Code == ReplyCode.Succeed) 
                    {
                        _myOrders.TryRemove(id, out _);
                    }
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} removed {5}", id, price, quantity, type, status, response.Response.Code);
                    return;
                }
                //если вновь пришедший ордер закрыт и он был на продажу, то просто удаляем его из нашего списка и больше не подгоняем его цену
                if (status == OrderStatus.Closed && oldOrder.Signature.Type == OrderType.Sell)
                {
                    PlaceOrderResponse response = await context.PlaceOrder(sellPrice, oldOrder.Quantity);
                    if (response.Response.Code == ReplyCode.Succeed)
                    {
                        _myOrders.TryRemove(id, out _);
                    }
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} removed {5}", id, price, quantity, type, status, response.Response.Code);
                    return;
                } 
                if (status == OrderStatus.Open && oldOrder.Signature.Type == OrderType.Buy)
                {
                    PlaceOrderResponse response = null;
                    if (quantity != 0) 
                    {
                        newQuantity = oldOrder.Quantity - quantity;
                        response = await context.PlaceOrder(sellPrice, -newQuantity);
                        if (response.Response.Code == ReplyCode.Succeed)
                        {
                            _myOrders.AddOrUpdate(id, newComingOrder, (k, v) =>
                            {
                                if (price != 0) v.Price = price;
                                if (quantity != 0) v.Quantity = quantity;
                                return v;
                            });
                        }
                        Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} updated {5}", id, price, quantity, type, status, response.Response.Code);
                    }
                    return;
                }
                if (status == OrderStatus.Open && oldOrder.Signature.Type == OrderType.Sell)
                {
                    PlaceOrderResponse response = null;
                    if (quantity != 0)
                    {
                        newQuantity = oldOrder.Quantity + quantity;
                        response = await context.PlaceOrder(sellPrice, newQuantity);
                        if (response.Response.Code == ReplyCode.Succeed)
                        {
                            _myOrders.AddOrUpdate(id, newComingOrder, (k, v) =>
                            {
                                if (price != 0) v.Price = price;
                                if (quantity != 0) v.Quantity = quantity;
                                return v;
                            });
                        }
                        Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} updated {5}", id, price, quantity, type, status, response.Response.Code);
                    }
                    return;
                } 
            }
        }

        /// <summary>
        /// Формирует ордер на покупку
        /// </summary>
        public async Task FormPurchaseOrder(UserContext context)
        {
            if (CheckContext(context)) return;
            double quantity = 0;
            double availableBalance = ConvertSatoshiToXBT(_availableBalance) * context.configuration.AvaibleBalance;
            double totalBalance = ConvertSatoshiToXBT(_totalBalance) * context.configuration.AvaibleBalance;
            //вычисляем рыночную цену для продажи
            double purchaseFairPrice = _purchaseOrderBook.Max(x => x.Value.Price);

            //если наша позиция длинная, то есть _positionSize имеет положительное значение, то мы можем продавать только по доступному балансу
            //если наша позиция короткая, то есть _positionSize имеет отрицательное значение, то мы можем продавать по общему балансу, который есть на аккаунте
            if (_positionSize >= 0) quantity = context.configuration.ContractValue * Math.Floor(availableBalance * purchaseFairPrice / context.configuration.ContractValue);
            else quantity = context.configuration.ContractValue * Math.Floor(totalBalance * purchaseFairPrice / context.configuration.ContractValue);

            //если баланса было достаточно для хотя бы одного ордера то выполняем продажу
            if (quantity > 0) 
            {
                PlaceOrderResponse response = await context.PlaceOrder(purchaseFairPrice, quantity);
                if(response.Response.Code == ReplyCode.Succeed)
                {
                    _myOrders.TryAdd(response.OrderId, new Order
                    {
                        Id = response.OrderId,
                        Price = purchaseFairPrice,
                        Quantity = quantity,
                        Signature = new OrderSignature
                        {
                            Status = OrderStatus.Open,
                            Type = OrderType.Buy
                        },
                        LastUpdateDate = new Google.Protobuf.WellKnownTypes.Timestamp()
                    });
                }
                Log.Information("Order price: {0}, quantity: {1} placed {2}", purchaseFairPrice, quantity, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Failure ? response.Response.Message : "");
            }
            else Log.Debug("Cannot place purchase order. Insufficient balance.");
        }

        /// <summary>
        /// Формирует ордер на продажу
        /// </summary>
        public async Task FormSellOrder(UserContext context)
        {
            if (CheckContext(context)) return;
            double quantity = 0;
            double availableBalance = ConvertSatoshiToXBT(_availableBalance) * context.configuration.AvaibleBalance;
            double totalBalance = ConvertSatoshiToXBT(_totalBalance) * context.configuration.AvaibleBalance;
            //вычисляем рыночную цену для продажи
            double sellFairPrice = _sellOrderBook.Min(x => x.Value.Price);

            //если наша позиция короткая, то есть _positionSize имеет отрицательное значение, то мы можем продавать только по доступному балансу
            //если наша позиция длинная, то есть _positionSize имеет положительное значение, то мы можем продавать по общему балансу, который есть на аккаунте
            if (_positionSize <= 0) quantity = context.configuration.ContractValue * Math.Floor(availableBalance * sellFairPrice / context.configuration.ContractValue);
            else quantity = context.configuration.ContractValue * Math.Floor(totalBalance * sellFairPrice / context.configuration.ContractValue);

            //если баланса было достаточно для хотя бы одного ордера то выполняем продажу
            if (quantity > 0)
            {
                PlaceOrderResponse response = await context.PlaceOrder(sellFairPrice, -quantity);
                if (response.Response.Code == ReplyCode.Succeed)
                {
                    _myOrders.TryAdd(response.OrderId, new Order
                    {
                        Id = response.OrderId,
                        Price = sellFairPrice,
                        Quantity = -quantity,
                        Signature = new OrderSignature
                        {
                            Status = OrderStatus.Open,
                            Type = OrderType.Sell
                        },
                        LastUpdateDate = new Google.Protobuf.WellKnownTypes.Timestamp()
                    });
                    Log.Information("Order price: {0}, quantity: {1} placed {2}", sellFairPrice, -quantity, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Failure ? response.Response.Message : "");
                }
            }
            else Log.Debug("Cannot place sell order. Insufficient balance.");
        }

        /// <summary>
        /// Конвертирует сатоши в биткоины
        /// </summary>
        /// <param name="satoshiValue"></param>
        /// <returns></returns>
        private double ConvertSatoshiToXBT(int satoshiValue) 
        {
            return satoshiValue * 0.00000001;
        }

        /// <summary>
        /// Возвращает true если контекст нулевой или имеет нулевые поля
        /// </summary>
        private bool CheckContext(UserContext context)
        {
            if (context is null)
            {
                Log.Error("Bad user context (null)");
                return true;
            }
            if (context.configuration is null || context.sessionId is null || context.slot is null || context.trademarket is null)
            {
                Log.Error("Bad user context (some field is null)");
                return true;
            }
            return false;
        }
    }
}
