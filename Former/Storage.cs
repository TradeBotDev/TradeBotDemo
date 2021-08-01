using Serilog;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;

namespace Former
{
    public class Storage
    {
        private readonly ConcurrentDictionary<string, Order> _buyOrderBook;
        private readonly ConcurrentDictionary<string, Order> _sellOrderBook;
        private readonly ConcurrentDictionary<string, Order> _myOrders;
        private int _totalBalance;
        private int _availableBalance;
        private int _positionSize;
        private int _positionSizeInActiveOrders;
        private double _sellFairPrice;
        private double _buyFairPrice;
        private readonly int _bookSize;

        public Storage(int bookSize)
        {
            _sellOrderBook = new ConcurrentDictionary<string, Order>();
            _buyOrderBook = new ConcurrentDictionary<string, Order>();
            _myOrders = new ConcurrentDictionary<string, Order>();
            _bookSize = bookSize;
        }

        /// <summary>
        /// Обновляет доступный и общий баланс
        /// </summary>
        internal Task UpdateBalance(int availableBalance, int totalBalance)
        {
            if (_availableBalance != availableBalance)
            {
                Log.Information("Balance updated. Available balance: {0}, Total balance: {1}", availableBalance, totalBalance);
                _availableBalance = availableBalance;
            }
            _totalBalance = totalBalance;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Обновляет размер позиции, для того чтобы знать, короткая позиция или длинная
        /// </summary>
        internal Task UpdatePosition(double currentQuantity)
        {
            if (_positionSize != (int)currentQuantity)
            {
                Log.Information("Current position: {0}, position in active orders: {1}", currentQuantity, _positionSizeInActiveOrders);
                _positionSize = (int)currentQuantity;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Обновляет конкретную книгу ордеров
        /// </summary>
        private async Task UpdateConcreteBook(ConcurrentDictionary<string, Order> bookNeededUpdate, Order newComingOrder)
        {
            var updateConcreteBookTask = new Task(() =>
            {
                //если ордер имеет статус открытый, то он добавляется, либо апдейтится, если закрытый, то удаляется из книги
                if (newComingOrder.Signature.Status == OrderStatus.Open)
                    bookNeededUpdate.AddOrUpdate(newComingOrder.Id, newComingOrder, (_, v) =>
                    {
                        //у ордера на обновление не нулевой будет величина, которая изменилась
                        if (newComingOrder.Price != 0) v.Price = newComingOrder.Price;
                        if (newComingOrder.Quantity != 0) v.Quantity = newComingOrder.Quantity;
                        v.Signature = newComingOrder.Signature;
                        v.LastUpdateDate = newComingOrder.LastUpdateDate;
                        v.Signature = newComingOrder.Signature;
                        return v;
                    });
                else if (bookNeededUpdate.ContainsKey(newComingOrder.Id))
                    bookNeededUpdate.TryRemove(newComingOrder.Id, out _);
            });
            await updateConcreteBookTask;
        }

        /// <summary>
        /// Обновляет одну из книг ордеров с вновь пришедшим ордером, в зависимости от того, какого типа ордер необходимо внести в книгу
        /// </summary>
        internal async Task UpdateOrderBooks(Order newComingOrder, UserContext context)
        {
            var task = new Task(async () =>
            {
                //выбирает, какую книгу апдейтить
                await UpdateConcreteBook(newComingOrder.Signature.Type == OrderType.Buy ? _buyOrderBook : _sellOrderBook, newComingOrder);
                await UpdateFairPrices();
            });
            await task;
        }

        /// <summary>
        /// Обновляет рыночные цены на продажу и на покупку
        /// </summary>
        private async Task UpdateFairPrices()
        {
            //вычиляем рыночные цены на покупку и на продажу и выполняем проверку на актуальность наших ордеров
            var sellFairPrice = _sellOrderBook.Min(x => x.Value.Price);
            var buyFairPrice = _buyOrderBook.Max(x => x.Value.Price);
            await CheckAndFitPrices(buyFairPrice, sellFairPrice);
        }

        /// <summary>
        /// Проверяет, стоит ли перевыставлять ордера, и вызывает FitPrices, в том случае, если это необходимо
        /// </summary>
        private async Task CheckAndFitPrices(double buyFairPrice, double sellFairPrice)
        {
            //если рыночная цена изменилась, то необходимо проверить, не устарели ли цени в наших ордерах
            if ((int)Math.Floor(_sellFairPrice) != (int)Math.Floor(sellFairPrice) || (int)Math.Floor(_buyFairPrice) != (int)Math.Floor(buyFairPrice))
            {
                _sellFairPrice = sellFairPrice;
                _buyFairPrice = buyFairPrice;
                if (_buyOrderBook.Count >= _bookSize && _sellOrderBook.Count >= _bookSize && _myOrders.Count > 0) await FitPrices(context);
            }
        }

        /// <summary>
        /// Возвращает true, если получилось удалить ордер по идентификатору из списка моих ордеров, иначе false
        /// </summary>
        private bool RemoveFromMyOrders(string id)
        {
            return _myOrders.TryRemove(id, out _);
        }

        /// <summary>
        /// Обновляет запись в списке моих ордеров, если запись с таким же идентификатором существует там
        /// </summary>
        private void UpdateMyOrder(Order newComingOrder)
        {
            if (_myOrders.ContainsKey(newComingOrder.Id))
                _myOrders.AddOrUpdate(newComingOrder.Id, newComingOrder, (_, v) =>
                {
                    if (newComingOrder.Price != 0) v.Price = newComingOrder.Price;
                    if (newComingOrder.Quantity != 0) v.Quantity = -newComingOrder.Quantity;
                    v.LastUpdateDate = newComingOrder.LastUpdateDate;
                    v.Signature = newComingOrder.Signature;
                    return v;
                });
        }

        /// <summary>
        /// Обновляет список моих ордеров по подписке, и выставляет контр-ордер в случае частичного или полного исполнения моего ордера
        /// </summary>
        internal async Task UpdateMyOrderList(Order newComingOrder, ChangesType changesType, UserContext context)
        {
            //вновь пришедший ордер не помещается в список моих ордеров здесь, потому что это делается только по событию из алгоритма, во избежание
            //зацикливания выставления ордеров и контр-ордеров
            if (changesType == ChangesType.Insert) return;
            //по той же причине, что и выше, ордера, которые уже были на бирже не инициализируются снова, а лишь идут в расчёт текущей позиции на бирже
            if (changesType == ChangesType.Partitial)
            {
                _positionSizeInActiveOrders += (int)newComingOrder.Quantity;
                Log.Information("Position size in active orders has been updated: {0}", _positionSizeInActiveOrders);
                return;
            }

            //данная переменная действует, как семафор. Она предотвращает одновременное выставление контр ордера и выставление ордера по просьбе алгоритма, так как из за высвобождения 
            //средств оба эти действия имеют место.

            var id = newComingOrder.Id;

            //выходим из метода, если не получилось получить ордер по входящему идентификатору
            if (!_myOrders.TryGetValue(id, out var oldOrder)) return;

            //рассчитываем цены продажи/покупку для контр ордеров
            var sellPrice = oldOrder.Price + oldOrder.Price * context.Configuration.RequiredProfit;
            var buyPrice = oldOrder.Price - oldOrder.Price * context.Configuration.RequiredProfit;

            //если входящий ордер имеет пометку "удалить" необходимо выставить контр-ордер в полном объёме, и в случае, если это удастся, удалить его из списка моих ордеров
            if (changesType == ChangesType.Delete)
            {
                var placeResponse = await context.PlaceFullCounterOrder(oldOrder.Signature.Type == OrderType.Buy ? sellPrice : buyPrice, oldOrder.Quantity);
                var removeResponse = false;
                if (placeResponse.Response.Code == ReplyCode.Succeed)
                {
                    removeResponse = RemoveFromMyOrders(id);
                    _positionSizeInActiveOrders -= (int)oldOrder.Quantity;
                }
                Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} removed {5}", oldOrder.Id, oldOrder.Price, oldOrder.Quantity, removeResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                Log.Information("Counter order price: {0}, quantity: {1} placed {2} {3}", oldOrder.Signature.Type == OrderType.Buy ? sellPrice : buyPrice, -oldOrder.Quantity, placeResponse.Response.Code, placeResponse.Response.Code == ReplyCode.Succeed ? "" : placeResponse.Response.Message);
            }
            //если входящий ордер имеет пометку "обновить" необходимо обновить цену или объём в совпадающем по ид ордере, и в случае обновления объёма выставить контр-ордер с частичным объёмом
            {
                if (newComingOrder.Quantity != 0)
                {
                    var placeResponse = await context.PlacePartialCounterOrder(oldOrder.Signature.Type == OrderType.Buy ? buyPrice : sellPrice, newComingOrder.Quantity, oldOrder.Quantity);
                    if (placeResponse.Response.Code == ReplyCode.Succeed)
                    {
                        UpdateMyOrder(newComingOrder);
                        _positionSizeInActiveOrders -= (int)(newComingOrder.Quantity - oldOrder.Quantity);
                    }
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} updated", oldOrder.Id, oldOrder.Price, oldOrder.Quantity, oldOrder.Signature.Type);
                    Log.Information("Counter order price: {0}, quantity: {1} placed {2} {3}", oldOrder.Signature.Type == OrderType.Buy ? buyPrice : sellPrice, -oldOrder.Quantity, placeResponse.Response.Code, placeResponse.Response.Code == ReplyCode.Succeed ? "" : placeResponse.Response.Message);
                }
                if (newComingOrder.Price != 0)
                {
                    UpdateMyOrder(newComingOrder);
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} updated", id, newComingOrder.Price, -oldOrder.Quantity, oldOrder.Signature.Type);
                }
            }
        }
        //internal async Task UpdateMyOrderList(Order newComingOrder, ChangesType changesType, UserContext context)
        //{
        //    //вновь пришедший ордер не помещается в список моих ордеров здесь, потому что это делается только по событию из алгоритма, во избежание
        //    //зацикливания выставления ордеров и контр-ордеров
        //    if (changesType == ChangesType.Insert) return;
        //    //по той же причине, что и выше, ордера, которые уже были на бирже не инициализируются снова, а лишь идут в расчёт текущей позиции на бирже
        //    if (changesType == ChangesType.Partitial)
        //    {
        //        _positionSizeInActiveOrders += (int)newComingOrder.Quantity;
        //        Log.Information("Position size in active orders has been updated: {0}", _positionSizeInActiveOrders);
        //        return;
        //    }
        //    if (CheckContext(context)) return;

        //    //данная переменная действует, как семафор. Она предотвращает одновременное выставление контр ордера и выставление ордера по просьбе алгоритма, так как из за высвобождения 
        //    //средств оба эти действия имеют место.
        //    _placeLocker = true;
        //    _fitPricesLocker = true;

        //    var id = newComingOrder.Id;

        //    //выходим из метода, если не получилось получить ордер по входящему идентификатору
        //    if (!_myOrders.TryGetValue(id, out var oldOrder)) return;

        //    //рассчитываем цены продажи/покупку для контр ордеров
        //    var sellPrice = oldOrder.Price + oldOrder.Price * context.Configuration.RequiredProfit;
        //    var buyPrice = oldOrder.Price - oldOrder.Price * context.Configuration.RequiredProfit;

        //    //если входящий ордер имеет пометку "удалить" необходимо выставить контр-ордер в полном объёме, и в случае, если это удастся, удалить его из списка моих ордеров
        //    if (changesType == ChangesType.Delete)
        //    {
        //        var placeResponse = await PlaceFullCounterOrder(oldOrder.Signature.Type == OrderType.Buy ? sellPrice : buyPrice, oldOrder.Quantity, context);
        //        var removeResponse = false;
        //        if (placeResponse.Response.Code == ReplyCode.Succeed)
        //        {
        //            removeResponse = RemoveFromMyOrders(id);
        //            _positionSizeInActiveOrders -= (int)oldOrder.Quantity;
        //        }
        //        Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} removed {5}", oldOrder.Id, oldOrder.Price, oldOrder.Quantity, removeResponse ? ReplyCode.Succeed : ReplyCode.Failure);
        //        Log.Information("Counter order price: {0}, quantity: {1} placed {2} {3}", oldOrder.Signature.Type == OrderType.Buy ? sellPrice : buyPrice, -oldOrder.Quantity, placeResponse.Response.Code, placeResponse.Response.Code == ReplyCode.Succeed ? "" : placeResponse.Response.Message);
        //    }
        //    //если входящий ордер имеет пометку "обновить" необходимо обновить цену или объём в совпадающем по ид ордере, и в случае обновления объёма выставить контр-ордер с частичным объёмом
        //    {
        //        if (newComingOrder.Quantity != 0)
        //        {
        //            var placeResponse = await PlacePartialCounterOrder(oldOrder.Signature.Type == OrderType.Buy ? buyPrice : sellPrice, newComingOrder.Quantity, oldOrder.Quantity, context);
        //            if (placeResponse.Response.Code == ReplyCode.Succeed)
        //            {
        //                UpdateMyOrder(newComingOrder);
        //                _positionSizeInActiveOrders -= (int)(newComingOrder.Quantity - oldOrder.Quantity);
        //            }
        //            Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} updated", oldOrder.Id, oldOrder.Price, oldOrder.Quantity, oldOrder.Signature.Type);
        //            Log.Information("Counter order price: {0}, quantity: {1} placed {2} {3}", oldOrder.Signature.Type == OrderType.Buy ? buyPrice : sellPrice, -oldOrder.Quantity, placeResponse.Response.Code, placeResponse.Response.Code == ReplyCode.Succeed ? "" : placeResponse.Response.Message);
        //        }
        //        if (newComingOrder.Price != 0)
        //        {
        //            UpdateMyOrder(newComingOrder);
        //            Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} updated", id, newComingOrder.Price, -oldOrder.Quantity, oldOrder.Signature.Type);
        //        }
        //    }
        //    _placeLocker = false;
        //    _fitPricesLocker = false;
        //}

        /// <summary>
        /// Подгоняет мои ордера под рыночную цену
        /// </summary>
        private async Task FitPrices(UserContext context)
        {
            foreach (var (key, order) in _myOrders)
            {
                if (order.Signature.Type == OrderType.Sell)
                    if (order.Price - _sellFairPrice > context.Configuration.OrderUpdatePriceRange)
                    {
                        if (_sellFairPrice - 1 > _buyFairPrice) _sellFairPrice -= 2;
                        var response = await context.AmendOrder(order.Id, _sellFairPrice + 1);
                        if (response.Response.Code == ReplyCode.Succeed)
                            _myOrders.AddOrUpdate(key, order, (_, v) =>
                            {
                                v.Price = _sellFairPrice + 1;
                                v.Quantity = v.Quantity;
                                v.LastUpdateDate = v.LastUpdateDate;
                                v.Signature = v.Signature;
                                return v;
                            });
                        Log.Information("Order {0} amended with {1} {2} {3}", key, _sellFairPrice, response.Response.Code.ToString(), response.Response.Message);
                    }
                if (order.Signature.Type == OrderType.Buy)
                    if (_buyFairPrice - order.Price > context.Configuration.OrderUpdatePriceRange)
                    {
                        if (_buyFairPrice + 1 > _sellFairPrice) _buyFairPrice += 2;
                        var response = await context.AmendOrder(order.Id, _buyFairPrice - 1);
                        if (response.Response.Code == ReplyCode.Succeed)
                            _myOrders.AddOrUpdate(key, order, (_, v) =>
                            {
                                v.Price = _buyFairPrice - 1;
                                v.Quantity = v.Quantity;
                                v.LastUpdateDate = v.LastUpdateDate;
                                v.Signature = v.Signature;
                                return v;
                            });
                        Log.Information("Order {0} amended with {1} {2} {3}", key, _buyFairPrice, response.Response.Code.ToString(), response.Response.Message);
                    }
            }
        }
    }
}
