using Serilog;
using System;
using System.Collections.Concurrent;
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

        private int _positionSizeInActiveOrders;

        private int _positionSize;

        private int _bookSize;

        private bool _placeLocker;

        private bool _fitLocker;

        public Former(int bookSize)
        {
            _sellOrderBook = new ConcurrentDictionary<string, Order>();
            _purchaseOrderBook = new ConcurrentDictionary<string, Order>();
            _myOrders = new ConcurrentDictionary<string, Order>();
            _bookSize = bookSize;
        }

        #region UpdateRegion

        /// <summary>
        /// Обновляет конкретную книгу ордеров
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
                }
                if (newComingOrder.Signature.Type == OrderType.Sell)
                {
                    await UpdateConcreteBook(_sellOrderBook, newComingOrder, context);
                    //если книга ордеров полностью заполнилась, вычисление рыночной цены, выполняющейся в методе FitPrices, будет корректным
                }
                if (_purchaseOrderBook.Count >= _bookSize && _sellOrderBook.Count >= _bookSize && !_fitLocker)
                    await FitPrices(context);
            });
            await task;
        }

        /// <summary>
        /// Обновляет размер позиции, для того чтобы знать, короткая позиция или длинная
        /// </summary>
        internal Task UpdatePosition(double currentQuantity)
        {
            if (_positionSize != currentQuantity) Log.Information("Current postition: {0}, position in active orders: {1}", currentQuantity, _positionSizeInActiveOrders);

            _positionSize = (int)currentQuantity;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Обновляет доступный и общий баланс
        /// </summary>
        internal Task UpdateBalance(int availableBalance, int totalBalance)
        {
            if (_availableBalance != availableBalance) Log.Information("Balance updated. Available balance: {0}, Total balance: {1}", availableBalance, totalBalance);
            _availableBalance = availableBalance;
            _totalBalance = totalBalance;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Обновляет список моих ордеров по подписке
        /// </summary>
        internal async Task UpdateMyOrderList(Order newComingOrder, ChangesType changesType, UserContext context)
        {
            if (CheckContext(context)) return;
            Order oldOrder = null;
            var id = newComingOrder.Id;
            var price = newComingOrder.Price;
            var quantity = newComingOrder.Quantity;
            var type = newComingOrder.Signature.Type;
            var status = newComingOrder.Signature.Status;
            
            double newQuantity = 0;
            if (changesType == ChangesType.Partitial)
            {
                _positionSizeInActiveOrders += (int)newComingOrder.Quantity;
                Log.Information("Position size in active orders has been updated: {0}", _positionSizeInActiveOrders);
                return;
            }

            if (changesType == ChangesType.Insert || !_myOrders.TryGetValue(id, out oldOrder)) return;

            double sellPrice = oldOrder.Price + oldOrder.Price * context.configuration.RequiredProfit;
            double purchasePrice = oldOrder.Price - oldOrder.Price * context.configuration.RequiredProfit;

            _placeLocker = true;
            if (changesType == ChangesType.Delete)
            {
                if (oldOrder.Signature.Type == OrderType.Buy)
                {
                    PlaceOrderResponse response = await context.PlaceOrder(sellPrice, -oldOrder.Quantity);
                    if (response.Response.Code == ReplyCode.Succeed)
                    {
                        _positionSizeInActiveOrders -= (int)oldOrder.Quantity;
                        _myOrders.TryRemove(id, out _);
                    }
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} removed {5}", id, price, -oldOrder.Quantity, type, status, response.Response.Code);
                    Log.Information("Counter order price: {0}, quantity: {1} placed for sell {2} {3}", sellPrice, -oldOrder.Quantity, response.Response.Code.ToString(), response.Response.Message);
                }
                if (oldOrder.Signature.Type == OrderType.Sell)
                {
                    PlaceOrderResponse response = await context.PlaceOrder(purchasePrice, -oldOrder.Quantity);
                    if (response.Response.Code == ReplyCode.Succeed)
                    {
                        _positionSizeInActiveOrders -= (int)oldOrder.Quantity;
                        _myOrders.TryRemove(id, out _);
                    }
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} removed {5}", id, price, -oldOrder.Quantity, type, status, response.Response.Code);
                    Log.Information("Counter order price: {0}, quantity: {1} placed for purchase {2} {3}", purchasePrice, -oldOrder.Quantity, response.Response.Code.ToString(), response.Response.Message);
                }
            }
            if (changesType == ChangesType.Update)
            {
                if (oldOrder.Signature.Type == OrderType.Buy)
                {
                    if (quantity != 0)
                    {
                        newQuantity = oldOrder.Quantity - quantity;
                        PlaceOrderResponse response = await context.PlaceOrder(sellPrice, -newQuantity);
                        _positionSizeInActiveOrders -= (int)newQuantity;
                        Log.Information("Counter order price: {0}, quantity: {1} placed for sell {2} {3}", sellPrice, -newQuantity, response.Response.Code.ToString(), response.Response.Message);
                    }
                    _myOrders.AddOrUpdate(id, newComingOrder, (k, v) =>
                    {
                        if (price != 0) v.Price = price;
                        if (quantity != 0) v.Quantity = -quantity;
                        return v;
                    });
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} updated {5}", id, price, quantity, type, status);
                }
                if (oldOrder.Signature.Type == OrderType.Sell)
                {
                    if (quantity != 0)
                    {
                        newQuantity = oldOrder.Quantity - quantity;
                        PlaceOrderResponse response = await context.PlaceOrder(purchasePrice, -newQuantity);
                        _positionSizeInActiveOrders += (int)newQuantity;
                        Log.Information("Counter order price: {0}, quantity: {1} placed for purchase {2} {3}", purchasePrice, -newQuantity, response.Response.Code.ToString(), response.Response.Message);
                    }
                    _myOrders.AddOrUpdate(id, newComingOrder, (k, v) =>
                    {
                        if (price != 0) v.Price = price;
                        if (quantity != 0) v.Quantity = quantity;
                        return v;
                    });
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} updated {5}", id, price, quantity, type, status);
                }
            }
            _placeLocker = false;
        }

        #endregion

        #region FormRegion
        /// <summary>
        /// Формирует ордер на покупку
        /// </summary>
        public async Task FormPurchaseOrder(UserContext context)
        {
            if (_placeLocker) return;
            if (CheckContext(context)) return;
            double quantity = 0;
            double availableBalance = ConvertSatoshiToXBT(_availableBalance) * context.configuration.AvaibleBalance;
            double totalBalance = ConvertSatoshiToXBT(_totalBalance) * context.configuration.AvaibleBalance;
            //вычисляем рыночную цену для продажи
            double purchaseFairPrice = _purchaseOrderBook.Max(x => x.Value.Price);

            //если наша позиция длинная, то есть _positionSize имеет положительное значение, то мы можем продавать только по доступному балансу
            //если наша позиция короткая, то есть _positionSize имеет отрицательное значение, то мы можем продавать по общему балансу, который есть на аккаунте
            if (_positionSize >= 0) quantity = context.configuration.ContractValue * Math.Floor(availableBalance * purchaseFairPrice / context.configuration.ContractValue);
            else quantity = context.configuration.ContractValue * Math.Floor(totalBalance * purchaseFairPrice / context.configuration.ContractValue) - _positionSizeInActiveOrders;

            //если баланса было достаточно для хотя бы одного ордера то выполняем продажу
            if (quantity > 0)
            {
                PlaceOrderResponse response = await context.PlaceOrder(purchaseFairPrice - 1, quantity);
                if (response.Response.Code == ReplyCode.Succeed)
                {
                    _myOrders.TryAdd(response.OrderId, new Order
                    {
                        Id = response.OrderId,
                        Price = purchaseFairPrice - 1,
                        Quantity = quantity,
                        Signature = new OrderSignature
                        {
                            Status = OrderStatus.Open,
                            Type = OrderType.Buy
                        },
                        LastUpdateDate = new Google.Protobuf.WellKnownTypes.Timestamp()
                    });
                    _positionSizeInActiveOrders += (int)quantity;
                }
                Log.Information("Order price: {0}, quantity: {1} placed for purchase {2} {3}", purchaseFairPrice - 1, quantity, response.Response.Code.ToString(), response.Response.Message);
            }
            else Log.Debug("Cannot place purchase order. Insufficient balance.");
        }

        /// <summary>
        /// Формирует ордер на продажу
        /// </summary>
        public async Task FormSellOrder(UserContext context)
        {
            if (_placeLocker) return;
            if (CheckContext(context)) return;
            double quantity = 0;
            double availableBalance = ConvertSatoshiToXBT(_availableBalance) * context.configuration.AvaibleBalance;
            double totalBalance = ConvertSatoshiToXBT(_totalBalance) * context.configuration.AvaibleBalance;
            //вычисляем рыночную цену для продажи
            double sellFairPrice = _sellOrderBook.Min(x => x.Value.Price);

            //если наша позиция короткая, то есть _positionSize имеет отрицательное значение, то мы можем продавать только по доступному балансу
            //если наша позиция длинная, то есть _positionSize имеет положительное значение, то мы можем продавать по общему балансу, который есть на аккаунте
            if (_positionSize <= 0) quantity = context.configuration.ContractValue * Math.Floor(availableBalance * sellFairPrice / context.configuration.ContractValue);
            else quantity = context.configuration.ContractValue * Math.Floor(totalBalance * sellFairPrice / context.configuration.ContractValue) + _positionSizeInActiveOrders;

            //если баланса было достаточно для хотя бы одного ордера то выполняем продажу
            if (quantity > 0)
            {
                PlaceOrderResponse response = await context.PlaceOrder(sellFairPrice + 1, -quantity);
                if (response.Response.Code == ReplyCode.Succeed)
                {
                    _myOrders.TryAdd(response.OrderId, new Order
                    {
                        Id = response.OrderId,
                        Price = sellFairPrice + 1,
                        Quantity = -quantity,
                        Signature = new OrderSignature
                        {
                            Status = OrderStatus.Open,
                            Type = OrderType.Sell
                        },
                        LastUpdateDate = new Google.Protobuf.WellKnownTypes.Timestamp()
                    });
                }
                Log.Information("Order price: {0}, quantity: {1} placed for sell {2} {3}", sellFairPrice + 1, -quantity, response.Response.Code.ToString(), response.Response.Message);
                _positionSizeInActiveOrders -= (int)quantity;
            }
            else Log.Debug("Cannot place sell order. Insufficient balance.");
        }
        #endregion

        #region UtilityRegion

        /// <summary>
        /// Подгоняет мои ордера под рыночную цену
        /// </summary>
        private async Task FitPrices(UserContext context)
        {
            _fitLocker = true;
            if (CheckContext(context)) return;
            var checkPrices = Task.Run(async () =>
            {
                double sellFairPrice = _sellOrderBook.Min(x => x.Value.Price);
                double purchaseFairPrice = _purchaseOrderBook.Max(x => x.Value.Price);

                foreach (var order in _myOrders)
                {
                    if (order.Value.Signature.Type == OrderType.Sell)
                        if (order.Value.Price - sellFairPrice > context.configuration.OrderUpdatePriceRange)
                        {
                            if (sellFairPrice - 1 > purchaseFairPrice) sellFairPrice -= 1;
                            AmmendOrderResponse response = await context.SetNewPrice(order.Value.Id, sellFairPrice + 1);
                            if (response.Response.Code == ReplyCode.Succeed)
                                _myOrders.AddOrUpdate(order.Key, order.Value, (k, v) =>
                                {
                                    v.Price = sellFairPrice + 1;
                                    return v;
                                });
                            Log.Information("Order {0} amended with {1} {2} {3}", order.Key, sellFairPrice, response.Response.Code.ToString(), response.Response.Message);
                        }
                    if (order.Value.Signature.Type == OrderType.Buy)
                        if (purchaseFairPrice - order.Value.Price > context.configuration.OrderUpdatePriceRange)
                        {
                            if (purchaseFairPrice + 1 > sellFairPrice) purchaseFairPrice += 1;
                            AmmendOrderResponse response = await context.SetNewPrice(order.Value.Id, sellFairPrice - 1);
                            if (response.Response.Code == ReplyCode.Succeed)
                                _myOrders.AddOrUpdate(order.Key, order.Value, (k, v) =>
                                {
                                    v.Price = sellFairPrice - 1;
                                    return v;
                                });
                            Log.Information("Order {0} amended with {1} {2} {3}", order.Key, sellFairPrice, response.Response.Code.ToString(), response.Response.Message);
                        }
                }
            });
            await checkPrices;
            _fitLocker = false;
        }

        /// <summary>
        /// Конвертирует сатоши в биткоины
        /// </summary>
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
        #endregion
    }
}
