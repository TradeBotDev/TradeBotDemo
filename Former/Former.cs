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
                Log.Information("Buy market price: {0}, Sell market price: {1}", _buyMarketPrice, _sellMarketPrice);
                if (!_fitPricesLocker && !_myOrders.IsEmpty) await FitPrices(context);
            }
        }

        /// <summary>
        /// Обновляет цену ордера из _myOrders
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
                    var removeResponse = RemoveOrder(key, _myOrders);
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} removed cause cannot be amended {4} ", order.Id, order.Price, order.Quantity, order.Signature.Type, removeResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                }
                Log.Information("Order {0} amended with {1} {2} {3}", key, fairPrice, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Succeed ? "" : response.Response.Message);
            }
            _fitPricesLocker = false;
        }
        /// <summary>
        /// Возвращает true, если получилось удалить ордер по идентификатору из выбранного списка, иначе false
        /// </summary>
        private bool RemoveOrder(string id, ConcurrentDictionary<string, Order> list)
        {
            return list.TryRemove(id, out _);
        }

        /// <summary>
        /// Обновляет запись в выбранном списке, если она там существует
        /// </summary>
        private bool UpdateOrder(Order newComingOrder, ConcurrentDictionary<string, Order> list)
        {
            if (!list.ContainsKey(newComingOrder.Id)) return false;
            list.AddOrUpdate(newComingOrder.Id, newComingOrder, (_, v) =>
            {
                if (newComingOrder.Price != 0) v.Price = newComingOrder.Price;
                if (newComingOrder.Quantity != 0) v.Quantity = newComingOrder.Quantity;
                v.LastUpdateDate = newComingOrder.LastUpdateDate;
                v.Signature = newComingOrder.Signature;
                v.LastUpdateDate = newComingOrder.LastUpdateDate;
                return v;
            });
            return true;
        }

        /// <summary>
        /// Добавляет запись в выбранный список 
        /// </summary>
        private bool AddOrder(string id, Order order, ConcurrentDictionary<string, Order> list)
        {
            return list.TryAdd(id, order);
        }

        private async Task PlaceCounterOrder(Order oldOrder, Order newComingOrder, UserContext context)
        {
            var quantity = oldOrder.Quantity - newComingOrder.Quantity;
            var price = oldOrder.Signature.Type == OrderType.Buy ? oldOrder.Price + oldOrder.Price * context.Configuration.RequiredProfit : oldOrder.Price - oldOrder.Price * context.Configuration.RequiredProfit;
            var type = oldOrder.Signature.Type == OrderType.Buy ? OrderType.Sell : OrderType.Buy;
            var addResponse = false;

            var placeResponse = await context.PlaceOrder(price, -quantity);
            if (placeResponse.Response.Code == ReplyCode.Succeed)
            {
                addResponse = AddOrder(placeResponse.OrderId,
                    new Order
                    {
                        Id = placeResponse.OrderId,
                        Price = price,
                        Quantity = -quantity,
                        Signature = new OrderSignature { Status = OrderStatus.Open, Type = type },
                        LastUpdateDate = new Timestamp()
                    }, _counterOrders);
            }
            Log.Information("Counter order {0} price: {1}, quantity: {2} placed {3} {4}", oldOrder.Id, price, -quantity, placeResponse.Response.Code, placeResponse.Response.Code == ReplyCode.Succeed ? "" : placeResponse.Response.Message);
            Log.Information("Order {0}, price: {1}, quantity: {2}, type: {3} added to counter orders list {4}", placeResponse.OrderId, price, -quantity, type, addResponse ? ReplyCode.Succeed : ReplyCode.Failure);
        }

        /// <summary>
        /// Обновляет список моих ордеров по подписке, и выставляет контр-ордер в случае частичного или полного исполнения моего ордера
        /// </summary>
        internal async Task UpdateMyOrderList(Order newComingOrder, ChangesType changesType, UserContext context)
        {
            if (CheckContext(context)) return;

            var id = newComingOrder.Id;
            var itsMyOrder = _myOrders.TryGetValue(id, out var myOldOrder);
            var itsCounterOrder = _counterOrders.TryGetValue(id, out var counterOldOrder);

            switch (changesType)
            {
                case ChangesType.Partitial:
                    AddOrder(id, newComingOrder, _counterOrders);
                    return;
                case ChangesType.Update when itsMyOrder:
                    _placeLocker = true;
                    _fitPricesLocker = true;
                    var updateMyOrderResponse = UpdateOrder(newComingOrder, _myOrders);
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} updated {4}", myOldOrder.Id, myOldOrder.Price, myOldOrder.Quantity, myOldOrder.Signature.Type, updateMyOrderResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                    if (newComingOrder.Quantity != 0) await PlaceCounterOrder(myOldOrder, newComingOrder, context);
                    _placeLocker = false;
                    _fitPricesLocker = false;
                    break;
                case ChangesType.Update when itsCounterOrder:
                    var updateCounterOrderResponse = UpdateOrder(newComingOrder, _counterOrders);
                    Log.Information("Counter order {0}, price: {1}, quantity: {2}, type: {3} updated {4}", counterOldOrder.Id, counterOldOrder.Price, counterOldOrder.Quantity, counterOldOrder.Signature.Type, updateCounterOrderResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                    break;
                case ChangesType.Delete when itsMyOrder:
                    _placeLocker = true;
                    _fitPricesLocker = true;
                    var removeResponse = RemoveOrder(id, _myOrders);
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} removed {4}", myOldOrder.Id, myOldOrder.Price, myOldOrder.Quantity, myOldOrder.Signature.Type, removeResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                    await PlaceCounterOrder(myOldOrder, newComingOrder, context);
                    _placeLocker = false;
                    _fitPricesLocker = false;
                    break;
                case ChangesType.Delete when itsCounterOrder:
                    RemoveOrder(id, _counterOrders);
                    break;
                //вновь пришедший ордер не помещается в список моих ордеров здесь, потому что это делается только по событию из алгоритма, во избежание
                //зацикливания выставления ордеров и контр-ордеров
                case ChangesType.Insert:
                    return;
                case ChangesType.Undefiend:
                    return;
                default:
                    return;
            }


        }

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
        /// Возвращает false, если текущий баланс и маржа не позволяют выставить ордер на покупку/продажу, иначе true
        /// </summary>
        private bool CheckPossibilityPlacingOrder(OrderType type, UserContext context)
        {
            var orderCost = context.Configuration.ContractValue / (type == OrderType.Sell ? _sellMarketPrice : _buyMarketPrice);
            var availableBalanceWithConfigurationReduction = ConvertSatoshiToXBT(_availableBalance) * context.Configuration.AvaibleBalance;
            var totalBalanceWithConfigurationReduction = ConvertSatoshiToXBT(_totalBalance) * context.Configuration.AvaibleBalance;

            double marginOfAlreadyPlacedSellOrders = 0;
            double marginOfAlreadyPlacedBuyOrders = 0;

            if (!_myOrders.IsEmpty || !_counterOrders.IsEmpty)
            {
                var marginOfMySellOrders = _myOrders.Where(x => x.Value.Signature.Type == OrderType.Sell).Sum(x => x.Value.Quantity / x.Value.Price);
                var marginOfCounterSellOrders = _counterOrders.Where(x => x.Value.Signature.Type == OrderType.Sell).Sum(x => x.Value.Quantity / x.Value.Price);
                marginOfAlreadyPlacedSellOrders = marginOfMySellOrders + marginOfCounterSellOrders;

                var marginOfMyBuyOrders = _myOrders.Where(x => x.Value.Signature.Type == OrderType.Buy).Sum(x => x.Value.Quantity / x.Value.Price);
                var marginOfCounterBuyOrders = _counterOrders.Where(x => x.Value.Signature.Type == OrderType.Buy).Sum(x => x.Value.Quantity / x.Value.Price);
                marginOfAlreadyPlacedBuyOrders = marginOfMyBuyOrders + marginOfCounterBuyOrders;
            }

            switch (_positionSize)
            {
                case < 0 when availableBalanceWithConfigurationReduction < orderCost:
                    Log.Debug("Cannot place {0} order. Insufficient available balance.", type);
                    return false;
                case >= 0 when totalBalanceWithConfigurationReduction + marginOfAlreadyPlacedSellOrders - marginOfAlreadyPlacedBuyOrders < orderCost:
                    Log.Debug("Cannot place {0} order. Insufficient total balance.", type);
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Формирует в зависимости от решения алгоритма
        /// </summary>
        public async Task FormOrder(int decision, UserContext context)
        {
            if (_placeLocker) return;
            if (CheckContext(context)) return;
            var orderType = decision > 0 ? OrderType.Buy : OrderType.Sell;
            if (!CheckPossibilityPlacingOrder(orderType, context)) return;

            var quantity = orderType == OrderType.Buy ? context.Configuration.ContractValue : -context.Configuration.ContractValue;
            var price = orderType == OrderType.Buy ? _buyMarketPrice : _sellMarketPrice;

            var response = await context.PlaceOrder(price, quantity);
            if (response.Response.Code == ReplyCode.Succeed)
            {
                var addResponse = AddOrder(response.OrderId,
                    new Order
                    {
                        Id = response.OrderId,
                        Price = price,
                        Quantity = quantity,
                        Signature = new OrderSignature{ Status = OrderStatus.Open, Type = orderType },
                        LastUpdateDate = new Timestamp()
                    }, _myOrders);
                Log.Information("Order {0}, price: {1}, quantity: {2}, type: {3} added to my orders list {4}", response.OrderId, price, quantity, orderType, addResponse ? ReplyCode.Succeed : ReplyCode.Failure);
            }
            Log.Information("Order {0} price: {1}, quantity: {2} placed for {3} {4} {5}", response.OrderId, price, quantity, orderType, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Failure ? response.Response.Message : "");
        }

        /// <summary>
        /// Конвертирует сатоши в биткоины
        /// </summary>
        private double ConvertSatoshiToXBT(int satoshiValue)
        {
            return satoshiValue * 0.00000001;
        }

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
