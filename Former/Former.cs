using Google.Protobuf.WellKnownTypes;
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
        private readonly ConcurrentDictionary<string, Order> _myOrders;

        private readonly ConcurrentDictionary<string, Order> _counterOrders;

        private int _totalBalance;

        private int _availableBalance;

        private int _positionSize;

        private bool _placeLocker;

        private bool _fitPricesLocker;
        
        private double _sellMarketPrice;

        private double _buyMarketPrice;

        private double _savedMarketBuyPrice;

        private double _savedMarketSellPrice;

        public Former()
        {
            _myOrders = new ConcurrentDictionary<string, Order>();
            _counterOrders = new ConcurrentDictionary<string, Order>();
        }

        #region Update order book and fit prices

        /// <summary>
        /// Обновляет рыночные цены на продажу и на покупку
        /// </summary>
        internal async Task UpdateMarketPrices(double bid, double ask, UserContext context)
        {
            if (bid > 0) _buyMarketPrice = bid;
            if (ask > 0) _sellMarketPrice = ask;
            await CheckAndFitPrices(context);
        }
        /// <summary>
        /// Проверяет, стоит ли перевыставлять ордера, и вызывает FitPrices, в том случае, если это необходимо
        /// </summary>
        private async Task CheckAndFitPrices(UserContext context)
        {
            //если рыночная цена изменилась, то необходимо проверить, не устарели ли цени в наших ордерах
            if (Math.Abs(_sellMarketPrice - _savedMarketSellPrice) > 0.4 || Math.Abs(_savedMarketBuyPrice - _buyMarketPrice) > 0.4)
            {
                _savedMarketSellPrice = _sellMarketPrice;
                _savedMarketBuyPrice = _buyMarketPrice;
                if (!_fitPricesLocker && _myOrders.Count > 0) await FitPrices(context);
            }
        }

        /// <summary>
        /// обновляет ордер в мапе _myOrders 
        /// </summary>
        private void UpdateOrderPrice(Order order, double price)
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

        private double GetFairPrice(OrderType type)
        {
            return type == OrderType.Sell ? _sellMarketPrice : _buyMarketPrice;
        }

        /// <summary>
        /// Подгоняет мои ордера под рыночную цену
        /// </summary>
        private async Task FitPrices(UserContext context)
        {
            if (CheckContext(context)) return;
            _fitPricesLocker = true;

            var ordersSuitableForUpdate = _myOrders.Where(pair => Math.Abs(pair.Value.Price - GetFairPrice(pair.Value.Signature.Type)) >= context.Configuration.OrderUpdatePriceRange);
            foreach (var (key, order) in ordersSuitableForUpdate)
            {
                var fairPrice = GetFairPrice(order.Signature.Type);
                var response = await context.AmendOrder(order.Id, fairPrice);

                if (response.Response.Code == ReplyCode.Succeed) UpdateOrderPrice(order, fairPrice);
                else if (response.Response.Message.Contains("Invalid ordStatus"))
                {
                    var removeResponse = RemoveFromMyOrders(key);
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} removed {4}", order.Id, order.Price, order.Quantity, order.Signature.Type, removeResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                }
                

                Log.Information("Order {0} amended with {1} {2} {3}", key, fairPrice, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Succeed ? "" : response.Response.Message);
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
                    v.LastUpdateDate = newComingOrder.LastUpdateDate;
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
            switch (changesType)
            {
                //вновь пришедший ордер не помещается в список моих ордеров здесь, потому что это делается только по событию из алгоритма, во избежание
                //зацикливания выставления ордеров и контр-ордеров
                case ChangesType.Insert:
                    return;
                //по той же причине, что и выше, ордера, которые уже были на бирже не инициализируются снова, а лишь идут в расчёт текущей позиции на бирже
                case ChangesType.Partitial:
                    _counterOrders.TryAdd(newComingOrder.Id, newComingOrder);
                    return;
            }

            if (CheckContext(context)) return;



            var id = newComingOrder.Id;

            //выходим из метода, если не получилось получить ордер по входящему идентификатору
            if (_myOrders.TryGetValue(id, out var myOldOrder))
            {

                _placeLocker = true;
                _fitPricesLocker = true;
                //рассчитываем цены продажи/покупку для контр ордеров
                var sellPrice = myOldOrder.Price + myOldOrder.Price * context.Configuration.RequiredProfit;
                var buyPrice = myOldOrder.Price - myOldOrder.Price * context.Configuration.RequiredProfit;

                switch (changesType)
                {
                    //если входящий ордер имеет пометку "удалить" необходимо выставить контр-ордер в полном объёме, и в случае, если это удастся, удалить его из списка моих ордеров
                    case ChangesType.Delete:
                        {
                            var placeResponse = await PlaceFullCounterOrder(myOldOrder.Signature.Type == OrderType.Buy ? sellPrice : buyPrice, myOldOrder.Quantity, context);
                            var removeResponse = false;
                            if (placeResponse.Response.Code == ReplyCode.Succeed)
                            {
                                removeResponse = RemoveFromMyOrders(id);

                                _counterOrders.TryAdd(placeResponse.OrderId, new Order
                                {
                                    Id = placeResponse.OrderId,
                                    Price = myOldOrder.Signature.Type == OrderType.Buy ? sellPrice : buyPrice,
                                    Quantity = -myOldOrder.Quantity,
                                    Signature = new OrderSignature
                                    {
                                        Status = OrderStatus.Open,
                                        Type = myOldOrder.Signature.Type == OrderType.Buy ? OrderType.Sell : OrderType.Buy,
                                    },
                                    LastUpdateDate = new Timestamp()
                                });
                            }
                            Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} removed {4}", myOldOrder.Id, myOldOrder.Price, myOldOrder.Quantity, myOldOrder.Signature.Type, removeResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                            Log.Information("Counter order {0} price: {1}, quantity: {2} placed {3} {4}", myOldOrder.Id, myOldOrder.Signature.Type == OrderType.Buy ? sellPrice : buyPrice, -myOldOrder.Quantity, placeResponse.Response.Code, placeResponse.Response.Code == ReplyCode.Succeed ? "" : placeResponse.Response.Message);
                            break;
                        }
                    //если входящий ордер имеет пометку "обновить" необходимо обновить цену или объём в совпадающем по ид ордере, и в случае обновления объёма выставить контр-ордер с частичным объёмом
                    case ChangesType.Update:
                        {
                            if (newComingOrder.Quantity != 0)
                            {
                                var placeResponse = await PlacePartialCounterOrder(myOldOrder.Signature.Type == OrderType.Buy ? buyPrice : sellPrice, newComingOrder.Quantity, myOldOrder.Quantity, context);
                                if (placeResponse.Response.Code == ReplyCode.Succeed)
                                {
                                    UpdateMyOrder(newComingOrder);
                                    _counterOrders.TryAdd(placeResponse.OrderId, new Order
                                    {
                                        Id = placeResponse.OrderId,
                                        Price = myOldOrder.Signature.Type == OrderType.Buy ? sellPrice : buyPrice,
                                        Quantity = -(myOldOrder.Quantity - newComingOrder.Quantity),
                                        Signature = new OrderSignature
                                        {
                                            Status = OrderStatus.Open,
                                            Type = myOldOrder.Signature.Type == OrderType.Buy ? OrderType.Sell : OrderType.Buy,
                                        },
                                        LastUpdateDate = new Timestamp()
                                    });
                                }
                                Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} updated", myOldOrder.Id, myOldOrder.Price, myOldOrder.Quantity, myOldOrder.Signature.Type);
                                Log.Information("Counter order {0} price: {1}, quantity: {2} placed {3} {4}", myOldOrder.Id, myOldOrder.Signature.Type == OrderType.Buy ? sellPrice : buyPrice, -(myOldOrder.Quantity - newComingOrder.Quantity), placeResponse.Response.Code, placeResponse.Response.Code == ReplyCode.Succeed ? "" : placeResponse.Response.Message);
                            }
                            if (newComingOrder.Price != 0)
                            {
                                UpdateMyOrder(newComingOrder);
                                Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} updated", id, newComingOrder.Price, -myOldOrder.Quantity, myOldOrder.Signature.Type);
                            }

                            break;
                        }
                }

                _placeLocker = false;
                _fitPricesLocker = false;
            }
            if (_counterOrders.TryGetValue(id, out _))
            {
                switch (changesType)
                {
                    case ChangesType.Delete:
                        RemoveFromMyOrders(id);
                        break;
                    case ChangesType.Update:
                        UpdateMyOrder(newComingOrder);
                        break;
                }
            }
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
                Log.Information("Current position: {0}", currentQuantity);
                _positionSize = (int)currentQuantity;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Обновляет доступный и общий баланс
        /// </summary>
        internal Task UpdateBalance(int availableBalance, int totalBalance)
        {
            if (availableBalance != 0)
            {
                _availableBalance = availableBalance;
                Log.Information("Balance updated. Available balance: {0}", availableBalance);
            }
            if (totalBalance != 0)
            {
                _totalBalance = totalBalance;
                Log.Information("Balance updated. Total balance: {0}", totalBalance);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Формирует ордер на покупку
        /// </summary>
        public async Task FormBuyOrder(UserContext context)
        {
            if (_placeLocker) return;
            if (CheckContext(context)) return;
            if (!CheckPossibilityPlacingOrder(OrderType.Buy, context)) return;

            var response = await context.PlaceOrder(_buyMarketPrice, context.Configuration.ContractValue);
            if (response.Response.Code == ReplyCode.Succeed)
            {
                _myOrders.TryAdd(response.OrderId, new Order
                {
                    Id = response.OrderId,
                    Price = _buyMarketPrice,
                    Quantity = context.Configuration.ContractValue,
                    Signature = new OrderSignature
                    {
                        Status = OrderStatus.Open,
                        Type = OrderType.Buy
                    },
                    LastUpdateDate = new Timestamp()
                });
            }
            Log.Information("Order {0} price: {1}, quantity: {2} placed for purchase {3} {4}", response.OrderId, _buyMarketPrice, context.Configuration.ContractValue, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Failure ? response.Response.Message : "");
        }

        /// <summary>
        /// Возвращает false, если текущий баланс и маржа не позволяют выставить ордер на покупку/продажу, иначе true
        /// </summary>
        private bool CheckPossibilityPlacingOrder(OrderType type, UserContext context)
        {
            var orderCost = context.Configuration.ContractValue / (type == OrderType.Sell ? _sellMarketPrice : _buyMarketPrice);
            var availableBalanceWithConfigurationReduction = ConvertSatoshiToXbt(_availableBalance) * context.Configuration.AvaibleBalance;
            var totalBalanceWithConfigurationReduction = ConvertSatoshiToXbt(_totalBalance) * context.Configuration.AvaibleBalance;

            var marginOfMySellOrders = _myOrders.Where(x => x.Value.Signature.Type == OrderType.Sell).Sum(x => x.Value.Quantity / x.Value.Price);
            var marginOfCounterSellOrders = _counterOrders.Where(x => x.Value.Signature.Type == OrderType.Sell).Sum(x => x.Value.Quantity / x.Value.Price);
            var marginOfAlreadyPlacedSellOrders = marginOfMySellOrders + marginOfCounterSellOrders;

            var marginOfMyBuyOrders = _myOrders.Where(x => x.Value.Signature.Type == OrderType.Buy).Sum(x => x.Value.Quantity / x.Value.Price);
            var marginOfCounterBuyOrders = _counterOrders.Where(x => x.Value.Signature.Type == OrderType.Buy).Sum(x => x.Value.Quantity / x.Value.Price);
            var marginOfAlreadyPlacedBuyOrders = marginOfMyBuyOrders + marginOfCounterBuyOrders;

            switch (_positionSize)
            {
                case < 0 when availableBalanceWithConfigurationReduction < orderCost:
                    Log.Debug("Cannot place {0} order. Insufficient available balance.", type);
                    return false;
                case > 0 when totalBalanceWithConfigurationReduction + marginOfAlreadyPlacedSellOrders - marginOfAlreadyPlacedBuyOrders < orderCost:
                    Log.Debug("Cannot place {0} order. Insufficient total balance.", type);
                    return false;
                case 0 when totalBalanceWithConfigurationReduction + marginOfAlreadyPlacedSellOrders - marginOfAlreadyPlacedBuyOrders < orderCost:
                    Log.Debug("Cannot place {0} order. Insufficient total balance.", type);
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Формирует ордер на продажу
        /// </summary>
        public async Task FormSellOrder(UserContext context)
        {
            if (_placeLocker) return;
            if (CheckContext(context)) return;
            if (!CheckPossibilityPlacingOrder(OrderType.Sell, context)) return;

            var response = await context.PlaceOrder(_sellMarketPrice, -context.Configuration.ContractValue);
            if (response.Response.Code == ReplyCode.Succeed)
            {
                _myOrders.TryAdd(response.OrderId, new Order
                {
                    Id = response.OrderId,
                    Price = _sellMarketPrice,
                    Quantity = -context.Configuration.ContractValue,
                    Signature = new OrderSignature
                    {
                        Status = OrderStatus.Open,
                        Type = OrderType.Sell
                    },
                    LastUpdateDate = new Timestamp()
                });
            }
            Log.Information("Order {0} price: {1}, quantity: {2} placed for sell {3} {4}", response.OrderId, _sellMarketPrice, -context.Configuration.ContractValue, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Failure ? response.Response.Message : "");
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
