using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Former
{
    public class Former
    {
        private readonly ConcurrentDictionary<string, Order> _purchaseOrderBook;

        private readonly ConcurrentDictionary<string, Order> _sellOrderBook;

        private readonly ConcurrentDictionary<string, Order> _myOrders;

        private double _balance;

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
        /// <param name="bookNeededUpdate"></param>
        /// <param name="whatBook"></param>
        /// <param name="order"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task UpdateConcreteBook(ConcurrentDictionary<string, Order> bookNeededUpdate, Order order, UserContext context)
        {
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
        /// <param name="orderNeededUpdate"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task UpdateOrderBooks(Order newComingOrder, UserContext context)
        {
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
                    if (_purchaseOrderBook.Count >= _bookSize)  await FitPrices(_sellOrderBook, OrderBookType.Sell, context);
                }
            });
            await task;
        }

        /// <summary>
        /// подгоняет мои ордера под рыночную цену (и на покупку и на продажу)
        /// </summary>
        /// <param name="ordersForFairPrice"></param>
        /// <param name="whatBook"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task FitPrices(ConcurrentDictionary<string, Order> ordersForFairPrice, OrderBookType bookType, UserContext context)
        {
            var checkPrices = Task.Run(async () =>
            {
                //в зависимости от того, какая книга обновлялась, вычисляется рыночная цена
                if (bookType == OrderBookType.Sell)
                {
                    double fairPrice = ordersForFairPrice.Min(x => x.Value.Price);
                    foreach (var order in _myOrders)
                    {
                        //проверяем все свои ордера, необходимо ли им подогнаться по рыночную цену
                        if (order.Value.Price > fairPrice)
                        {
                            var temp = order.Value;
                            order.Value.Price = fairPrice;
                            _myOrders.TryUpdate(order.Key, order.Value, temp);
                            //отправляется запрос в тм, чтобы он перевыставил ордер (поменял цену ордера) на новую (рыночную)
                            await context.SetNewPrice(order.Value);
                        }
                    }
                }
                if (bookType == OrderBookType.Buy)
                {
                    double fairPrice = ordersForFairPrice.Max(x => x.Value.Price);
                    foreach (var order in _myOrders)
                    {
                        if (order.Value.Price < fairPrice)
                        {
                            var temp = order.Value;
                            order.Value.Price = fairPrice;
                            _myOrders.TryUpdate(order.Key, order.Value, temp);
                            //отправляется запрос в тм, чтобы он перевыставил ордер (поменял цену ордера) на новую (рыночную)
                            await context.SetNewPrice(order.Value);
                        }
                    }
                }
            });
            await checkPrices;
        }

        /// <summary>
        /// обновляет балан для формирования верных списков 
        /// </summary>
        /// <param name="balance"></param>
        /// <returns></returns>
        public Task UpdateBalance(Balance balance)
        {
            Log.Information("Balance updated. New balance: {0}", balance.Value);
            _balance = double.Parse(balance.Value);
            return Task.CompletedTask;
        }

        /// <summary>
        /// обновляет список моих ордеров по подписке
        /// </summary>
        /// <param name="orderNeededUpdate"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task UpdateMyOrderList(Order newComingOrder, UserContext context)
        {
            Order oldOrder;
            var id = newComingOrder.Id;
            var price = newComingOrder.Price;
            var quantity = newComingOrder.Quantity;
            var type = newComingOrder.Signature.Type;
            var status = newComingOrder.Signature.Status;

            //пытается получить ордер из списка, если его нет, то добавляет его
            if (_myOrders.TryGetValue(id, out oldOrder))
            {
                //так как ордер сущетсвует в нашем списке, вероятно он закрыт почти или полностью и мы будем его продвать, поэтому сразу вычисляем цену для продажи
                double sellPrice = oldOrder.Price + oldOrder.Price * context.configuration.RequiredProfit;
                double newQuantity;

                if (status == OrderStatus.Closed && oldOrder.Signature.Type == OrderType.Buy)
                {
                    //если вновь пришедший ордер закрыт и он был на покупку, то запрашиваем продажу по вычисленной цене с отрицательным объёмом (нужно для тма, так он понимает, что это продажа)
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} removed", id, price, quantity, type, status);
                    await context.PlaceOrder(sellPrice, -oldOrder.Quantity);
                    _myOrders.TryRemove(id, out _);
                    return;
                }

                //если вновь пришедший ордер закрыт и он был на продажу, то просто удаляем его из нашего списка и больше не подгоняем его цену
                if (status == OrderStatus.Closed && oldOrder.Signature.Type == OrderType.Sell)
                {
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} fully removed", id, price, quantity, type, status);
                    _myOrders.TryRemove(id, out _);
                    return;
                } 

                if (quantity != 0 && (newQuantity = oldOrder.Quantity - quantity) > 0)
                {
                    //если вновь пришедний ордер имеет не нулевой объём, значит имеено объём был изменён, потому что не изменённые позиции приходят нулевыми или дефолтными
                    //поэтому следует обновить объём для этого ордера, а также продать часть ордера, которая была куплена 
                    Log.Information("My order {0}, price: {1}, new quantity: {2}, type: {3}, status: {4} partionally removed", id, price, quantity, type, status);
                    await context.PlaceOrder(sellPrice, -newQuantity);
                    _myOrders.TryUpdate(id, newComingOrder, oldOrder);
                    return;
                }
                //обновляем цену ордера, неважно на покупку он или на продажу 
                if (price != 0)
                {
                    Log.Information("My order {0}, new price: {1}, quantity: {2}, type: {3}, status: {4} price updated", id, price, quantity, type, status);
                    _myOrders.TryUpdate(id, newComingOrder, oldOrder);
                    return;
                } 
            }
            else
            {
                Log.Information("Order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} added to my orders", id, price, quantity, type, status);
                _myOrders.TryAdd(id, newComingOrder);
            }
        }

        /// <summary>
        /// формируется ордер на покупку
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FormPurchaseOrder(UserContext context)
        {
            Log.Debug("Playing long...");
            double availableBalance = _balance * context.configuration.AvaibleBalance;

            //Вычисляем рыночную цену, для выставления лимитного ордера
            double purchaseFairPrice = _purchaseOrderBook.Max(x => x.Value.Price);

            //вычисляем объём контракта, который может позволить наш баланс с учётом настройки доступности баланса 
            double quantity = context.configuration.ContractValue * Math.Floor(availableBalance * purchaseFairPrice / context.configuration.ContractValue);
            
            //купить ордер по рыночной цене, но ордер при этом лимитный 
            if (quantity > 0) await context.PlaceOrder(purchaseFairPrice, quantity);
            else Log.Debug("Insufficient balance");
        }

        /// <summary>
        /// формируется ордер на продажу
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FormSellOrder(UserContext context)
        {
            Log.Debug("Playing short...");

            //вычисляется доступный баланс в зависимости от настроек пользователя
            double availableBalance = _balance * context.configuration.AvaibleBalance;

            //вычисляем рыночную цену 
            double sellFairPrice = _sellOrderBook.Min(x => x.Value.Price);

            //вычисляем объём контракта, который может позволить наш баланс с учётом настройки доступности баланса 
            double quantity = context.configuration.ContractValue * Math.Floor(availableBalance * sellFairPrice / context.configuration.ContractValue);

            //продать ордер по рыночной цене, но ордер при этом лимитный 
            if (quantity > 0) await context.PlaceOrder(sellFairPrice, -quantity);
            else Log.Debug("Insufficient balance");
        }
    }
}
