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

        private double _totalBalance;

        private double _availableBalance;

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
        public static double RoundPriceRange(double sellPrice, double buyPrice)
        {
            return Math.Round(Math.Abs(sellPrice - buyPrice), 1);
        }

        //TODO test!!!
        public static double MakePrice(double range, double fairprice, OrderType type)
        {
            //TODO убедиться что рэнж точно идет с шагом 0.5
            return range > 0.5 ? fairprice + (type == OrderType.Buy ? 0.5 : -0.5) : fairprice;

        }

        /// <summary>
        /// обновляет ордер в мапе _myOrders 
        /// </summary>
        /// <param name="order"></param>
        /// <param name="price"></param>
        public void UpdateOrderPrice(Order order, double price)
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
            {
                AddMyOrder(newComingOrder);
            }
        }

        public void AddMyOrder(Order newComingOrder)
        {
            _myOrders.AddOrUpdate(newComingOrder.Id, newComingOrder, (_, v) =>
            {
                if (newComingOrder.Price != 0) v.Price = newComingOrder.Price;
                if (newComingOrder.Quantity != 0) v.Quantity = newComingOrder.Quantity;
                v.LastUpdateDate = newComingOrder.LastUpdateDate;
                v.Signature = newComingOrder.Signature;
                return v;
            });
        }

       


        public bool isOrderStored(string id)
        {
            return _myOrders.ContainsKey(id);
        }

        public double buildPrice(double oldPrice,double requeredProfit,OrderType type)
        {
            double additionalPrice = oldPrice * requeredProfit;
            var newPrice = oldPrice + (type == OrderType.Sell ? additionalPrice : -additionalPrice);
            //пока просто отбрасываем цену после запятой 
            return Math.Truncate(newPrice);
        }

        public async void UpdateOrderAndPlaceConterOrder(Order newComingOrder,UserContext context)
        {
            var orderType = newComingOrder.Signature.Type;
            var OldOrder = _myOrders[newComingOrder.Id];
            var quantityDifference = OldOrder.Quantity - newComingOrder.Quantity;

            //если разница по количеству больше нуля(волшебного числа)
            if (quantityDifference > 1e-5)
            {
                var newPrice = buildPrice(OldOrder.Price, context.Configuration.RequiredProfit, orderType);
                //выставляем контр ордер
                var response = await context.PlaceOrder(newPrice, quantityDifference);

                if (response.Response.Code == ReplyCode.Succeed)
                {
                    Log.Information("Order {@Id} {@Type} updated with {@Price} {@Quantity} ", newComingOrder.Id, orderType, newPrice, quantityDifference);
                    _positionSizeInActiveOrders -= (int)quantityDifference;
                }
            }

            UpdateMyOrder(newComingOrder);

        }

        /// <summary>
        /// Обновляет список моих ордеров по подписке, и выставляет контр-ордер в случае частичного или полного исполнения моего ордера
        /// </summary>
        internal async Task UpdateMyOrderList(Order newComingOrder, ChangesType changesType, UserContext context)
        {
            if (CheckContext(context)) return;
            await Task.Run(() =>
            {
                //данная переменная действует, как семафор. Она предотвращает одновременное выставление контр ордера и выставление ордера по просьбе алгоритма, так как из за высвобождения 
                //средств оба эти действия имеют место.
                _placeLocker = true;
                _fitPricesLocker = true;

                var id = newComingOrder.Id;

                switch (changesType)
                {
                    case ChangesType.Insert:
                        {
                            //вновь пришедший ордер не помещается в список моих ордеров здесь, потому что это делается только по событию из алгоритма, во избежание
                            //зацикливания выставления ордеров и контр-ордеров
                            break;
                        }
                    case ChangesType.Partitial:
                        {
                            //TEST FEATURE
                            //Все ордера пользователя которые были уже выставлены добавляются в список _MyOrders
                            AddMyOrder(newComingOrder);
                            break;
                        }
                    default:
                        {
                            UpdateOrderAndPlaceConterOrder(newComingOrder, context);
                            break;
                        }
                }
                _placeLocker = false;
                _fitPricesLocker = false;
            });
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
            if (_availableBalance != ConvertSatoshiToXbt(availableBalance))
            {
                Log.Information("Balance updated. Available balance: {0}, Total balance: {1}", availableBalance, totalBalance);
                _availableBalance = ConvertSatoshiToXbt(availableBalance);
            }
            _totalBalance = ConvertSatoshiToXbt(totalBalance);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Формирует ордер на покупку
        /// </summary>
        public async Task FormPurchaseOrder(UserContext context)
        {
            if (_placeLocker) return;
            if (CheckContext(context)) return;
            //вычисляем рыночную цену для продажи
            var purchaseFairPrice = _buyOrderBook.Max(x => x.Value.Price);
            
            if (_availableBalance - _totalBalance * context.Configuration.AvaibleBalance < context.Configuration.ContractValue / purchaseFairPrice)
            {
                Log.Debug("Cannot place sell order. Insufficient balance.");
                return;
            }

            //это хардкод ( - 1 ) . В ТМе не допускаются числа R для выставления ордеров
            var response = await context.PlaceOrder(purchaseFairPrice - 1, context.Configuration.ContractValue);
            if (response.Response.Code == ReplyCode.Succeed)
            {
                _myOrders.TryAdd(response.OrderId, new Order
                {
                    Id = response.OrderId,
                    Price = purchaseFairPrice - 1,
                    Quantity = context.Configuration.ContractValue,
                    Signature = new OrderSignature
                    {
                        Status = OrderStatus.Open,
                        Type = OrderType.Buy
                    },
                    LastUpdateDate = new Google.Protobuf.WellKnownTypes.Timestamp()
                });
                _positionSizeInActiveOrders += (int)context.Configuration.ContractValue;
            }
            Log.Information("Order price: {0}, quantity: {1} placed for purchase {2} {3}", purchaseFairPrice - 1, context.Configuration.ContractValue, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Failure ? response.Response.Message : "");

        }

        /// <summary>
        /// Формирует ордер на продажу
        /// </summary>
        public async Task FormSellOrder(UserContext context)
        {
            if (_placeLocker) return;
            if (CheckContext(context)) return;

            //вычисляем рыночную цену для продажи
            var sellFairPrice = _sellOrderBook.Min(x => x.Value.Price);

           
            if (_availableBalance - _totalBalance * context.Configuration.AvaibleBalance < context.Configuration.ContractValue / sellFairPrice)
            {
                Log.Debug("Cannot place sell order. Insufficient balance.");
                return;
            }
            var response = await context.PlaceOrder(sellFairPrice + 1, -context.Configuration.ContractValue);
            if (response.Response.Code == ReplyCode.Succeed)
            {
                _myOrders.TryAdd(response.OrderId, new Order
                {
                    Id = response.OrderId,
                    Price = sellFairPrice + 1,
                    Quantity = -context.Configuration.ContractValue,
                    Signature = new OrderSignature
                    {
                        Status = OrderStatus.Open,
                        Type = OrderType.Sell
                    },
                    LastUpdateDate = new Google.Protobuf.WellKnownTypes.Timestamp()
                });
            }
            Log.Information("Order price: {0}, quantity: {1} placed for sell {2} {3}", sellFairPrice + 1, -context.Configuration.ContractValue, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Failure ? response.Response.Message : "");
            _positionSizeInActiveOrders -= (int)context.Configuration.ContractValue;
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
