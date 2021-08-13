using System.Collections.Concurrent;
using System.Threading.Tasks;
using Serilog;
using TradeBot.Common.v1;

namespace Former.Model
{
    public class Storage
    {
        public delegate Task NeedPlaceOrderEvent(Order oldOrder, Order newComingOrder);
        public NeedPlaceOrderEvent PlaceOrderEvent;

        public delegate Task NeedHandleUpdate();
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

        internal Storage()
        {
            MyOrders = new ConcurrentDictionary<string, Order>();
            CounterOrders = new ConcurrentDictionary<string, Order>();
        }

        /// <summary>
        /// Обновляет рыночные цены на продажу и на покупку
        /// </summary>
        internal async Task UpdateMarketPrices(double bid, double ask)
        {
            if (bid > 0) BuyMarketPrice = bid;
            if (ask > 0) SellMarketPrice = ask;
            if (HandleUpdateEvent is not null) await HandleUpdateEvent.Invoke();
        }

        /// <summary>
        /// Обновляет список моих ордеров по подписке, и выставляет контр-ордер в случае частичного или полного исполнения моего ордера
        /// </summary>
        internal async Task UpdateMyOrderList(Order newComingOrder, ChangesType changesType)
        {
            var id = newComingOrder.Id;
            var itsMyOrder = MyOrders.TryGetValue(id, out var myOldOrder);
            var itsCounterOrder = CounterOrders.TryGetValue(id, out var counterOldOrder);

            switch (changesType)
            {
                case ChangesType.Partitial:
                    AddOrder(id, InitPartialOrder(newComingOrder), CounterOrders);
                    return;
                case ChangesType.Update when itsMyOrder:
                    var updateMyOrderResponse = UpdateOrder(newComingOrder, MyOrders);
                    Log.Information("{@Where}: My order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} updated {@ResponseCode}", "Former", myOldOrder.Id, myOldOrder.Price, myOldOrder.Quantity, myOldOrder.Signature.Type, updateMyOrderResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                    LockPlacingOrders(true);
                    if (newComingOrder.Quantity != 0)
                    {
                        if (PlaceOrderEvent is not null)
                        {
                            await PlaceOrderEvent?.Invoke(myOldOrder, newComingOrder);
                        }
                    }
                    LockPlacingOrders(false);
                    break;
                case ChangesType.Update when itsCounterOrder:
                    var updateCounterOrderResponse = UpdateOrder(newComingOrder, CounterOrders);
                    Log.Information("{@Where}: Counter order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} updated {@ResponseCode}", "Former", counterOldOrder.Id, counterOldOrder.Price, counterOldOrder.Quantity, counterOldOrder.Signature.Type, updateCounterOrderResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                    break;
                case ChangesType.Delete when itsMyOrder:
                    var removeMyOrderResponse = RemoveOrder(id, MyOrders);
                    Log.Information("{@Where}: My order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} removed {@ResponseCode}", "Former", myOldOrder.Id, myOldOrder.Price, myOldOrder.Quantity, myOldOrder.Signature.Type, removeMyOrderResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                    LockPlacingOrders(true);
                    if (PlaceOrderEvent is not null) await PlaceOrderEvent?.Invoke(myOldOrder, newComingOrder);
                    LockPlacingOrders(false);
                    break;
                case ChangesType.Delete when itsCounterOrder:
                    var removeCounterOrderResponse = RemoveOrder(id, CounterOrders);
                    Log.Information("{@Where}: Counter order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} removed {@ResponseCode}", "Former", counterOldOrder.Id, counterOldOrder.Price, counterOldOrder.Quantity, counterOldOrder.Signature.Type, removeCounterOrderResponse ? ReplyCode.Succeed : ReplyCode.Failure);
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
        internal Task UpdatePosition(double positionQuantity)
        {
            if (PositionSize != (int)positionQuantity)
            {
                Log.Information("{@Where}: Current position: {@Position}", "Former", positionQuantity);
                PositionSize = (int)positionQuantity;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Обновляет доступный и общий баланс
        /// </summary>
        internal Task UpdateBalance(int availableBalance, int totalBalance)
        {
            if (availableBalance > 0)
            {
                AvailableBalance = availableBalance;
                Log.Information("{@Where}: Balance updated. Available balance: {@AvailableBalance}, Total balance: {@TotalBalance}", "Former", availableBalance, totalBalance);
            }
            if (totalBalance > 0)
            {
                TotalBalance = totalBalance;
                HandleUpdateEvent?.Invoke();
            }
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Возвращает true, если получилось удалить ордер по идентификатору из выбранного списка, иначе false
        /// </summary>
        internal bool RemoveOrder(string id, ConcurrentDictionary<string, Order> list)
        {
            return list.TryRemove(id, out _);
        }

        /// <summary>
        /// Обновляет запись в выбранном списке, если она там существует
        /// </summary>
        internal bool UpdateOrder(Order newComingOrder, ConcurrentDictionary<string, Order> list)
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
        /// Из-за того, что инициализонные ордера приходят с биржи с положительным числом контрактов независимо от типа ордера, необходимо самому проинициализировать число контрактов
        /// </summary>
        private Order InitPartialOrder(Order newComingOrder)
        {
            return new Order
            {
                Id = newComingOrder.Id, 
                Price = newComingOrder.Price, 
                LastUpdateDate = newComingOrder.LastUpdateDate, 
                Signature = newComingOrder.Signature,
                Quantity = newComingOrder.Signature.Type == OrderType.Sell ? -newComingOrder.Quantity : newComingOrder.Quantity
            };

        }

        /// <summary>
        /// Добавляет запись в выбранный список 
        /// </summary>
        internal bool AddOrder(string id, Order order, ConcurrentDictionary<string, Order> list)
        {
            return list.TryAdd(id, order);
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
