using System;
using Serilog;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Former.Models
{
    public class Storage
    {
        internal delegate Task NeedPlaceOrderEvent(Order oldOrder, Order newComingOrder);
        internal NeedPlaceOrderEvent PlaceOrderEvent;

        internal delegate Task NeedHandleUpdate(Order order = null, ChangesType changesType = ChangesType.CHANGES_TYPE_UNDEFIEND);
        internal NeedHandleUpdate HandleUpdateEvent;

        internal readonly ConcurrentDictionary<string, Order> MyOrders;
        internal readonly ConcurrentDictionary<string, Order> CounterOrders;

        internal int TotalBalance;
        internal int AvailableBalance;

        internal int PositionSize;

        internal double SellMarketPrice;
        internal double BuyMarketPrice;

        internal bool PlaceLocker;
        internal bool FitPricesLocker;

        internal int LotSize;

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
            if (bid > 0)
            {
                BuyMarketPrice = bid;
                Log.Information("New buy market price {@BuyMarketPrice}", bid);
            }

            if (ask > 0)
            {
                SellMarketPrice = ask;
                Log.Information("New sell market price {@SellMarketPrice}", ask);
            }
            //необоходимо сообщить об изменениях UpdateHandler, чтобы тот проверил необходимость подгонки своих ордеров
            await HandleUpdateEvent.Invoke();
        }

        internal Task UpdateLotSize(int lotSize)
        {
            if (lotSize <= 0) return Task.CompletedTask;
            LotSize = lotSize;
            Log.Information("{@Where}: Lot size: {@LotSize}", "Former", lotSize);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Обновляет список моих ордеров по подписке, и выставляет контр-ордер в случае частичного или полного исполнения моего ордера
        /// </summary>
        internal async Task UpdateMyOrderList(Order newComingOrder, ChangesType changesType)
        {
            var id = newComingOrder.Id;
            //если пришедший ордер нашёлся в списке моих оредров, то это мой ордер, о чём сигнализирует переменная itsMyOrder
            var itsMyOrder = MyOrders.TryGetValue(id, out var myOldOrder);
            //если пришедший ордер нашёлся в списке контр оредров, то это контр-ордер, о чём сигнализирует переменная itsCounterOrder
            var itsCounterOrder = CounterOrders.TryGetValue(id, out var counterOldOrder);

            newComingOrder = InitOrderFromTM(newComingOrder);

            switch (changesType)
            {
                case ChangesType.CHANGES_TYPE_PARTITIAL:
                    //если ордер пришёл с пометкой Partitial, то это либо контр-ордер, либо мой ордер, который потерял связь и стал
                    //контр ордером. И в том и другом случае его необходимо проинициализировать, то есть добавить в список контр-ордеров
                    //и сообщить о его прибытии UpdateHandler, чтобы он его отправил в сервис истории.
                    AddOrder(id, newComingOrder, CounterOrders);
                    HandleUpdateEvent?.Invoke(newComingOrder, ChangesType.CHANGES_TYPE_PARTITIAL);
                    return;
                case ChangesType.CHANGES_TYPE_UPDATE when itsMyOrder:
                    //если оредр пришёл с пометкой Update и при этом является моим орером, то мы обновляем его в списке своих ордеров, а также
                    //если он имеет не нулевую, то это означает, что ордер исполнился частично и необходимо сообщить формеру о необходимости 
                    //выставить частичный контр-ордер (если ордер имеет нулевую Quantity, то обновилась цена)
                    var updateMyOrderResponse = UpdateOrder(newComingOrder, MyOrders);
                    Log.Information("{@Where}: My order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} updated {@ResponseCode}", "Former", myOldOrder.Id, myOldOrder.Price, myOldOrder.Quantity, myOldOrder.Signature.Type, updateMyOrderResponse ? ReplyCode.REPLY_CODE_SUCCEED : ReplyCode.REPLY_CODE_FAILURE);
                    LockPlacingOrders(true);
                    if (Math.Abs(newComingOrder.Quantity) < Math.Abs(myOldOrder.Quantity)) await PlaceOrderEvent.Invoke(myOldOrder, newComingOrder);
                    LockPlacingOrders(false);
                    break;
                case ChangesType.CHANGES_TYPE_UPDATE when itsCounterOrder:
                    //просто обновляется цена у контр-ордера
                    var updateCounterOrderResponse = UpdateOrder(newComingOrder, CounterOrders);
                    Log.Information("{@Where}: Counter order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} updated {@ResponseCode}", "Former", counterOldOrder.Id, counterOldOrder.Price, counterOldOrder.Quantity, counterOldOrder.Signature.Type, updateCounterOrderResponse ? ReplyCode.REPLY_CODE_SUCCEED : ReplyCode.REPLY_CODE_FAILURE);
                    break;
                case ChangesType.CHANGES_TYPE_DELETE when itsMyOrder:
                    //если оредр пришёл с пометкой Delete и при этом является моим орером, то необходимо удалить его из списка
                    //своих ордеров, и сообщить формеру о необходимости выставить полный контр-ордер
                    var removeMyOrderResponse = RemoveOrder(id, MyOrders);
                    Log.Information("{@Where}: My order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} removed {@ResponseCode}", "Former", myOldOrder.Id, myOldOrder.Price, myOldOrder.Quantity, myOldOrder.Signature.Type, removeMyOrderResponse ? ReplyCode.REPLY_CODE_SUCCEED : ReplyCode.REPLY_CODE_FAILURE);
                    LockPlacingOrders(true);
                    await PlaceOrderEvent.Invoke(myOldOrder, newComingOrder);
                    LockPlacingOrders(false);
                    break;
                case ChangesType.CHANGES_TYPE_DELETE when itsCounterOrder:
                    //если оредр пришёл с пометкой Delete и при этом является контр-орером, то необходимо удалить его из списка
                    //контр ордеров, и сообщить об этом UpdateHandler, чтобы он сообщил об этом истории.
                    var removeCounterOrderResponse = RemoveOrder(id, CounterOrders);
                    HandleUpdateEvent?.Invoke(newComingOrder, ChangesType.CHANGES_TYPE_DELETE);
                    Log.Information("{@Where}: Counter order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} removed {@ResponseCode}", "Former", counterOldOrder.Id, counterOldOrder.Price, counterOldOrder.Quantity, counterOldOrder.Signature.Type, removeCounterOrderResponse ? ReplyCode.REPLY_CODE_SUCCEED : ReplyCode.REPLY_CODE_FAILURE);
                    break;
                case ChangesType.CHANGES_TYPE_INSERT:
                    //пришедший ордер не помещается в список моих ордеров здесь, потому что это делается только по событию из алгоритма, во избежание
                    //зацикливания выставления ордеров и контр-ордеров
                    return;
                case ChangesType.CHANGES_TYPE_UNDEFIEND:
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
            if (PositionSize == (int)positionQuantity) return Task.CompletedTask;
            Log.Information("{@Where}: Current position: {@Position}", "Former", positionQuantity);
            PositionSize = (int)positionQuantity;
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
                Log.Information("{@Where}: Balance updated. Available balance: {@AvailableBalance}, Total balance: {@TotalBalance}", "Former", AvailableBalance, TotalBalance);
            }
            if (totalBalance > 0)
            {
                TotalBalance = totalBalance;
                //необходимо сообщить UpdateHandler, чтобы он передал обновлённый баланс в историю
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
        private Order InitOrderFromTM(Order newComingOrder)
        {
            return new Order
            {
                Id = newComingOrder.Id,
                Price = newComingOrder.Price,
                LastUpdateDate = newComingOrder.LastUpdateDate,
                Signature = newComingOrder.Signature,
                Quantity = newComingOrder.Signature.Type == OrderType.ORDER_TYPE_SELL ? -Math.Abs(newComingOrder.Quantity) : Math.Abs(newComingOrder.Quantity)
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
        /// <summary>
        /// Приводит все поля хранилища в исходное положение
        /// </summary>
        internal void ClearStorage()
        {
            MyOrders.Clear();
            CounterOrders.Clear();
            AvailableBalance = 0;
            SellMarketPrice = 0;
            TotalBalance = 0;
            PositionSize = 0;
            BuyMarketPrice = 0;
            FitPricesLocker = false;
            PlaceLocker = false;
        }
    }
}
