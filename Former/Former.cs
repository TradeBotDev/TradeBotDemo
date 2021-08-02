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
        private readonly ConcurrentDictionary<string, Order> _buyOrderBook;

        private readonly ConcurrentDictionary<string, Order> _sellOrderBook;

        private readonly ConcurrentDictionary<string, Order> _myOrders;

        private int _totalBalance;

        private int _availableBalance;

        private int _positionSizeInActiveOrders;

        private int _positionSize;

        private readonly int _bookSize;

        private bool _placeLocker;

        private bool _fitPricesLocker;

        private double _sellFairPrice;

        private double _buyFairPrice;

        public Former(int bookSize)
        {
            _sellOrderBook = new ConcurrentDictionary<string, Order>();
            _buyOrderBook = new ConcurrentDictionary<string, Order>();
            _myOrders = new ConcurrentDictionary<string, Order>();
            _bookSize = bookSize;
        }

        #region Update order book and fit prices
        /// <summary>
        /// Проверяет, стоит ли перевыставлять ордера, и вызывает FitPrices, в том случае, если это необходимо
        /// </summary>
        private async Task CheckAndFitPrices(double buyFairPrice, double sellFairPrice, UserContext context)
        {
            //если рыночная цена изменилась, то необходимо проверить, не устарели ли цени в наших ордерах
            if ((int)Math.Floor(_sellFairPrice) != (int)Math.Floor(sellFairPrice) || (int)Math.Floor(_buyFairPrice) != (int)Math.Floor(buyFairPrice))
            {
                _sellFairPrice = sellFairPrice;
                _buyFairPrice = buyFairPrice;
                if (!_fitPricesLocker && _myOrders.Count > 0) await FitPrices(context);
            }
        }

        /// <summary>
        /// Обновляет рыночные цены на продажу и на покупку
        /// </summary>
        private async Task UpdateFairPrices(UserContext context)
        {
            if (_buyOrderBook.Count < _bookSize || _sellOrderBook.Count < _bookSize) return;
            //вычиляем рыночные цены на покупку и на продажу и выполняем проверку на актуальность наших ордеров
            var sellFairPrice = _sellOrderBook.Min(x => x.Value.Price);
            var buyFairPrice = _buyOrderBook.Max(x => x.Value.Price);
            await CheckAndFitPrices(buyFairPrice, sellFairPrice, context);
        }

        /// <summary>
        /// Обновляет конкретную книгу ордеров
        /// </summary>
        private async Task UpdateConcreteBook(ConcurrentDictionary<string, Order> bookNeededUpdate, Order newComingOrder)
        {
            var updateConcreteBookTask = Task.Run(() =>
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
        public async Task UpdateOrderBooks(Order newComingOrder, UserContext context)
        {
            if (CheckContext(context)) return;
            var task = Task.Run(async () =>
            {
                //выбирает, какую книгу апдейтить
                await UpdateConcreteBook(newComingOrder.Signature.Type == OrderType.Buy ? _buyOrderBook : _sellOrderBook, newComingOrder);
                await UpdateFairPrices(context);
            });
            await task;
        }

        //TODO test!!!
        public static double RoundPriceRange(double sellPrice,double buyPrice)
        {
            return Math.Round(Math.Abs(sellPrice - buyPrice), 1);
        }

        //TODO test!!!
        public static double MakePrice(double range,double fairprice,OrderType type)
        {
            //TODO убедиться что рэнж точно идет с шагом 0.5
            return range > 0.5 ? fairprice + (type == OrderType.Buy ? 0.5 : -0.5) : fairprice;
            
        }

        /// <summary>
        /// обновляет ордер в мапе _myOrders 
        /// </summary>
        /// <param name="order"></param>
        /// <param name="price"></param>
        public void UpdateOrderPrice(Order order,double price)
        {
            _myOrders.AddOrUpdate(order.Id, order, (_, v) =>
            {
                v.Price = price;
                v.Quantity = v.Quantity;
                v.LastUpdateDate = v.LastUpdateDate;
                v.Signature = v.Signature;
                return v;
            });
        }
        /// <summary>
        /// Подгоняет мои ордера под рыночную цену
        /// </summary>
        private async Task FitPrices(UserContext context)
        {
            if (CheckContext(context)) return;
            _fitPricesLocker = true;
            double range = RoundPriceRange(_sellFairPrice, _buyFairPrice);

            foreach (var (key, order) in _myOrders)
            {
                var price = MakePrice(
                    range, 
                    order.Signature.Type == OrderType.Sell ? _sellFairPrice : _buyFairPrice, 
                    order.Signature.Type);

                var response = await context.AmendOrder(order.Id, price);

                if (response.Response.Code == ReplyCode.Succeed)
                    UpdateOrderPrice(order, price);
                Log.Information("Order {0} amended with {1} {2} {3}", key, price, response.Response.Code.ToString(), response.Response.Message);
            }
            _fitPricesLocker = false;
        }
        #endregion

        #region Update my order and place counter orders
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
                    //TODO тут удалили минус на количестве
                    if (newComingOrder.Quantity != 0) v.Quantity = newComingOrder.Quantity;
                    v.LastUpdateDate = newComingOrder.LastUpdateDate;
                    v.Signature = newComingOrder.Signature;
                    return v;
                });
        }

        /// <summary>
        /// Выставляет контр-ордер в полном объёме от изначального ордера
        /// </summary>
        private async Task<PlaceOrderResponse> PlaceFullCounterOrder(double price, double quantity, UserContext context)
        {
            var response = await context.PlaceOrder(price, -quantity);
            return response;
        }

        /// <summary>
        /// Выставляет контр-ордер с частью объёма от изначального ордера
        /// </summary>
        private async Task<PlaceOrderResponse> PlacePartialCounterOrder(double price, double newQuantity, double oldQuantity, UserContext context)
        {
            var quantity = oldQuantity - newQuantity;
            var response = await context.PlaceOrder(price, -quantity);
            return response;
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
            if (CheckContext(context)) return;

            //данная переменная действует, как семафор. Она предотвращает одновременное выставление контр ордера и выставление ордера по просьбе алгоритма, так как из за высвобождения 
            //средств оба эти действия имеют место.
            _placeLocker = true;
            _fitPricesLocker = true;

            var id = newComingOrder.Id;

            //выходим из метода, если не получилось получить ордер по входящему идентификатору
            if (!_myOrders.TryGetValue(id, out var oldOrder)) return;

            //рассчитываем цены продажи/покупку для контр ордеров
            var sellPrice = oldOrder.Price + oldOrder.Price * context.Configuration.RequiredProfit;
            var buyPrice = oldOrder.Price - oldOrder.Price * context.Configuration.RequiredProfit;

            //если входящий ордер имеет пометку "удалить" необходимо выставить контр-ордер в полном объёме, и в случае, если это удастся, удалить его из списка моих ордеров
            if (changesType == ChangesType.Delete)
            {
                var placeResponse = await PlaceFullCounterOrder(oldOrder.Signature.Type == OrderType.Buy ? sellPrice : buyPrice, oldOrder.Quantity, context);
                var removeResponse = false;
                if (placeResponse.Response.Code == ReplyCode.Succeed)
                {
                    removeResponse = RemoveFromMyOrders(id);
                    //если позиция была положительная то количество позиции отнимется от общего количества в ордерах.
                    _positionSizeInActiveOrders -= (int)oldOrder.Quantity;
                }
                Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} removed {5}", oldOrder.Id, oldOrder.Price, oldOrder.Quantity, removeResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                Log.Information("Counter order price: {0}, quantity: {1} placed {2} {3}", oldOrder.Signature.Type == OrderType.Buy ? sellPrice : buyPrice, -oldOrder.Quantity, placeResponse.Response.Code, placeResponse.Response.Code == ReplyCode.Succeed ? "" : placeResponse.Response.Message);
            }
            //если входящий ордер имеет пометку "обновить" необходимо обновить цену или объём в совпадающем по ид ордере, и в случае обновления объёма выставить контр-ордер с частичным объёмом
            if(changesType == ChangesType.Update)
            {
                if (newComingOrder.Quantity != 0)
                {
                    var placeResponse = await PlacePartialCounterOrder(oldOrder.Signature.Type == OrderType.Buy ? buyPrice : sellPrice, newComingOrder.Quantity, oldOrder.Quantity, context);
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
            _placeLocker = false;
            _fitPricesLocker = false;
        }
        #endregion

        #region Form orders and update balance and position
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
        /// Формирует ордер на покупку
        /// </summary>
        public async Task FormPurchaseOrder(UserContext context)
        {
            if (_placeLocker) return;
            if (CheckContext(context)) return;
            double quantity;
            var availableBalance = ConvertSatoshiToXbt(_availableBalance) * context.Configuration.AvaibleBalance;
            var totalBalance = ConvertSatoshiToXbt(_totalBalance) * context.Configuration.AvaibleBalance;
            //вычисляем рыночную цену для продажи
            var purchaseFairPrice = _buyOrderBook.Max(x => x.Value.Price);

            //если наша позиция длинная, то есть _positionSize имеет положительное значение, то мы можем продавать только по доступному балансу
            //если наша позиция короткая, то есть _positionSize имеет отрицательное значение, то мы можем продавать по общему балансу, который есть на аккаунте
            if (_positionSize >= 0) quantity = context.Configuration.ContractValue * Math.Floor(availableBalance * purchaseFairPrice / context.Configuration.ContractValue);
            else quantity = context.Configuration.ContractValue * Math.Floor(totalBalance * purchaseFairPrice / context.Configuration.ContractValue) - _positionSizeInActiveOrders;

            //если баланса было достаточно для хотя бы одного ордера то выполняем продажу
            if (quantity > 0)
            {
                //это хардкод ( - 1 ) . В ТМе не допускаются числа R для выставления ордеров
                var response = await context.PlaceOrder(purchaseFairPrice - 1, quantity);
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
                Log.Information("Order price: {0}, quantity: {1} placed for purchase {2} {3}", purchaseFairPrice - 1, quantity, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Failure ? response.Response.Message : "");
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
            double quantity;
            var availableBalance = ConvertSatoshiToXbt(_availableBalance) * context.Configuration.AvaibleBalance;
            var totalBalance = ConvertSatoshiToXbt(_totalBalance) * context.Configuration.AvaibleBalance;
            //вычисляем рыночную цену для продажи
            var sellFairPrice = _sellOrderBook.Min(x => x.Value.Price);

            //если наша позиция короткая, то есть _positionSize имеет отрицательное значение, то мы можем продавать только по доступному балансу
            //если наша позиция длинная, то есть _positionSize имеет положительное значение, то мы можем продавать по общему балансу, который есть на аккаунте
            if (_positionSize <= 0) quantity = context.Configuration.ContractValue * Math.Floor(availableBalance * sellFairPrice / context.Configuration.ContractValue);
            else quantity = context.Configuration.ContractValue * Math.Floor(totalBalance * sellFairPrice / context.Configuration.ContractValue) + _positionSizeInActiveOrders;

            //если баланса было достаточно для хотя бы одного ордера то выполняем продажу
            if (quantity > 0)
            {
                var response = await context.PlaceOrder(sellFairPrice + 1, -quantity);
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
                Log.Information("Order price: {0}, quantity: {1} placed for sell {2} {3}", sellFairPrice + 1, -quantity, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Failure ? response.Response.Message : "");
                _positionSizeInActiveOrders -= (int)quantity;
            }
            else Log.Debug("Cannot place sell order. Insufficient balance.");
        }

        /// <summary>
        /// Конвертирует сатоши в биткоины
        /// </summary>
        private double ConvertSatoshiToXbt(int satoshiValue)
        {
            return satoshiValue * 0.00000001;
        }
        #endregion

        /// <summary>
        /// Возвращает true если контекст нулевой или имеет нулевые поля, false иначе
        /// </summary>
        private bool CheckContext(UserContext context)
        {
            if (context is null)
            {
                Log.Error("Bad user context (null)");
                return true;
            }
            if (context.Configuration is null || context.SessionId is null || context.Slot is null || context.TradeMarket is null)
            {
                Log.Error("Bad user context (some field is null)");
                return true;
            }
            return false;
        }
    }
}
