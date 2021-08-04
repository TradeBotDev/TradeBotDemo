﻿using Serilog;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;

namespace Former
{
    public class Storage
    {
        public delegate Task NeedPlaceOrderEvent(Order oldOrder, Order newComingOrder, UserContext context);
        public NeedPlaceOrderEvent PlaceOrderEvent;

        public delegate Task NeedHandleUpdate(UserContext context);
        public NeedHandleUpdate HandleUpdateEvent;

        public readonly ConcurrentDictionary<string, Order> MyOrders;
        public readonly ConcurrentDictionary<string, Order> CounterOrders;

        public int TotalBalance;
        public int AvailableBalance;

        public int PositionSize;
        
        public double SellMarketPrice;
        public double BuyMarketPrice;

        public bool PlaceLocker;
        public bool FitPricesLocker;

        public double SavedMarketBuyPrice;
        public double SavedMarketSellPrice;

        public Storage()
        {
            MyOrders = new ConcurrentDictionary<string, Order>();
            CounterOrders = new ConcurrentDictionary<string, Order>();
        }

        /// <summary>
        /// Обновляет рыночные цены на продажу и на покупку
        /// </summary>
        internal async Task UpdateMarketPrices(double bid, double ask, UserContext context)
        {
            if (CheckContext(context)) return;

            if (bid > 0) BuyMarketPrice = bid;
            if (ask > 0) SellMarketPrice = ask;
            await HandleUpdateEvent.Invoke(context);
        }

        /// <summary>
        /// Обновляет список моих ордеров по подписке, и выставляет контр-ордер в случае частичного или полного исполнения моего ордера
        /// </summary>
        internal async Task UpdateMyOrderList(Order newComingOrder, ChangesType changesType, UserContext context)
        {
            if (CheckContext(context)) return;

            var id = newComingOrder.Id;
            var itsMyOrder = MyOrders.TryGetValue(id, out var myOldOrder);
            var itsCounterOrder = CounterOrders.TryGetValue(id, out var counterOldOrder);

            switch (changesType)
            {
                case ChangesType.Partitial:
                    AddOrder(id, newComingOrder, CounterOrders);
                    return;
                case ChangesType.Update when itsMyOrder:
                    var updateMyOrderResponse = UpdateOrder(newComingOrder, MyOrders);
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} updated {4}", myOldOrder.Id, myOldOrder.Price, myOldOrder.Quantity, myOldOrder.Signature.Type, updateMyOrderResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                    LockPlacingOrders(true);
                    if (newComingOrder.Quantity != 0) await PlaceOrderEvent.Invoke(myOldOrder, newComingOrder, context);
                    LockPlacingOrders(false);
                    break;
                case ChangesType.Update when itsCounterOrder:
                    var updateCounterOrderResponse = UpdateOrder(newComingOrder, CounterOrders);
                    Log.Information("Counter order {0}, price: {1}, quantity: {2}, type: {3} updated {4}", counterOldOrder.Id, counterOldOrder.Price, counterOldOrder.Quantity, counterOldOrder.Signature.Type, updateCounterOrderResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                    break;
                case ChangesType.Delete when itsMyOrder:
                    var removeResponse = RemoveOrder(id, MyOrders);
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} removed {4}", myOldOrder.Id, myOldOrder.Price, myOldOrder.Quantity, myOldOrder.Signature.Type, removeResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                    LockPlacingOrders(true);
                    await PlaceOrderEvent.Invoke(myOldOrder, newComingOrder, context);
                    LockPlacingOrders(false);
                    break;
                case ChangesType.Delete when itsCounterOrder:
                    RemoveOrder(id, CounterOrders);
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
            if (PositionSize != (int)currentQuantity)
            {
                Log.Information("Current position: {0}", currentQuantity);
                PositionSize = (int)currentQuantity;
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
                AvailableBalance = availableBalance;
                Log.Information("Balance updated. Available balance: {0}", availableBalance);
            }
            if (totalBalance != 0)
            {
                TotalBalance = totalBalance;
                Log.Information("Balance updated. Total balance: {0}", totalBalance);
            }
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Возвращает true, если получилось удалить ордер по идентификатору из выбранного списка, иначе false
        /// </summary>
        public bool RemoveOrder(string id, ConcurrentDictionary<string, Order> list)
        {
            return list.TryRemove(id, out _);
        }

        /// <summary>
        /// Обновляет запись в выбранном списке, если она там существует
        /// </summary>
        public bool UpdateOrder(Order newComingOrder, ConcurrentDictionary<string, Order> list)
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
        public bool AddOrder(string id, Order order, ConcurrentDictionary<string, Order> list)
        {
            return list.TryAdd(id, order);
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

        /// <summary>
        /// Блокирует возможность выставления ордера по просьбе алгоритма, а также изменение цены ордера в случае изменения рыночных цен
        /// </summary>
        private void LockPlacingOrders(bool needLock)
        {
            PlaceLocker = needLock;
            FitPricesLocker = needLock;
        }
    }
}
