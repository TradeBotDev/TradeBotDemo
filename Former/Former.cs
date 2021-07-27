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

        private double _balance = 0.0131;

        //TODO формер не должне принимать конфиг как аргмунет конструктора, но должен принимать конфиг ( или контекст пользователя) как аргумент своих методов
        public Former()
        {
            _sellOrderBook = new ConcurrentDictionary<string, Order>();
            _purchaseOrderBook = new ConcurrentDictionary<string, Order>();
            _myOrders = new ConcurrentDictionary<string, Order>();
        }
        /// <summary>
        /// апдейтит конкретную книгу ордеров (на покупку или продажу)
        /// </summary>
        /// <param name="bookNeededUpdate"></param>
        /// <param name="whatBook"></param>
        /// <param name="order"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task UpdateConcreteBook(ConcurrentDictionary<string, Order> bookNeededUpdate, string whatBook, Order order, UserContext context)
        {
            var task = Task.Run(async () =>
            {
                //если ордер имеет статус открытый, то он добавляется, либо апдейтится, если закрытый, то удаляется из книги
                if (order.Signature.Status == OrderStatus.Open)
                    bookNeededUpdate.AddOrUpdate(order.Id, order, (k, v) =>
                    {
                        var price = order.Price;
                        if (price != 0) v.Price = price;
                        v.Quantity = order.Quantity;
                        v.Signature = order.Signature;
                        v.LastUpdateDate = order.LastUpdateDate;
                        return v;
                    });
                else if (bookNeededUpdate.ContainsKey(order.Id))
                    bookNeededUpdate.TryRemove(order.Id, out _);

                //так как стаканы ордеров обновились, могла измениться рыночная цена на покупку или продажу и поэтому необходимо подгонать под неё мои ордера
                await FitPrices(bookNeededUpdate, whatBook, context);
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
                if (newComingOrder.Signature.Type == OrderType.Buy) await UpdateConcreteBook(_purchaseOrderBook, "purchase", newComingOrder, context);
                if (newComingOrder.Signature.Type == OrderType.Sell) await UpdateConcreteBook(_sellOrderBook, "sell", newComingOrder, context);
            });
            await task;
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

            //пытается получить ордер из списка, если его нет, то добавляет его
            if (_myOrders.TryGetValue(id, out oldOrder))
            {
                //так как ордер сущетсвует в нашем списке, вероятно он закрыт почти или полностью и мы будем его продвать, поэтому сразу вычисляем цену для продажи
                double sellPrice = oldOrder.Price + oldOrder.Price * context.configuration.RequiredProfit;
                double newQuantity;

                if (newComingOrder.Signature.Status == OrderStatus.Closed && oldOrder.Signature.Type == OrderType.Buy)
                {
                    //если вновь пришедший ордер закрыт и он был на покупку, то запрашиваем продажу по вычисленной цене с отрицательным объёмом (нужно для тма, так он понимает, что это продажа)
                    Log.Information("Order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} removed", id, newComingOrder.Price, newComingOrder.Quantity, newComingOrder.Signature.Type, newComingOrder.Signature.Status);
                    await context.PlaceOrder(sellPrice, -oldOrder.Quantity);
                    _myOrders.TryRemove(id, out _);
                }

                //если вновь пришедший ордер закрыт и он был на продажу, то просто удаляем его из нашего списка и больше не подгоняем его цену
                if (newComingOrder.Signature.Status == OrderStatus.Closed && oldOrder.Signature.Type == OrderType.Sell) _myOrders.TryRemove(id, out _);

                if (newComingOrder.Quantity != 0 && (newQuantity = oldOrder.Quantity - newComingOrder.Quantity) > 0)
                {
                    //если вновь пришедний ордер имеет не нулевой объём, значит имеено объём был изменён, потому что не изменённые позиции приходят нулевыми или дефолтными
                    //поэтому следует обновить объём для этого ордера, а также продать часть ордера, которая была куплена 
                    Log.Information("Order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} updated", id, newComingOrder.Price, newComingOrder.Quantity, newComingOrder.Signature.Type, newComingOrder.Signature.Status);
                    await context.PlaceOrder(sellPrice, -newQuantity);
                    _myOrders.TryUpdate(id, newComingOrder, oldOrder);
                }
                //обновляем цену ордера, неважно на покупку он или на продажу 
                if (newComingOrder.Price != 0) _myOrders.TryUpdate(id, newComingOrder, oldOrder);
            }
            else
            {
                Log.Information("Order {0}, price: {1}, quantity: {2}, type: {3}, status: {4} added to my orders", id, newComingOrder.Price, newComingOrder.Quantity, newComingOrder.Signature.Type, newComingOrder.Signature.Status);
                _myOrders.TryAdd(newComingOrder.Id, newComingOrder);
            }
        }

        /// <summary>
        /// подгоняет мои ордера под рыночную цену (и на покупку и на продажу)
        /// </summary>
        /// <param name="ordersForFairPrice"></param>
        /// <param name="whatBook"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task FitPrices(ConcurrentDictionary<string, Order> ordersForFairPrice, string whatBook, UserContext context)
        {
            //????????????????????????????????????????????????
            double fairPrice = 0;
            var ordersNeededToFit = new Dictionary<string, double>();

            var checkPrices = Task.Run(async () =>
            {
                //в зависимости от того, какая книга обновлялась, вычисляется рыночная цена
                if (whatBook == "sell") fairPrice = ordersForFairPrice.Min(x => x.Value.Price);
                if (whatBook == "purchase") fairPrice = ordersForFairPrice.Max(x => x.Value.Price);
                //проверяем все свои ордера, необходимо ли им подогнаться по рыночную цену
                foreach (var order in _myOrders)
                {
                    if (order.Value.Price < fairPrice)
                    {
                        order.Value.Price = fairPrice;
                        _myOrders.TryUpdate(order.Key, order.Value, order.Value);
                        //отправляется запрос в тм, чтобы он перевыставил ордер (поменял цену ордера) на новую (рыночную)
                        await context.SetNewPrice(order.Value);
                    }
                }
            });
            await checkPrices;
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
            
            //покупаем ордер по рыночной цене, но ордер при этом лимитный 
            await context.PlaceOrder(purchaseFairPrice, quantity);

            //временные манипуляции с балансом, вообще он должен обновляеться 
            _balance -= context.configuration.ContractValue / purchaseFairPrice;
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

            //отправляем запрос на продажу ордера по рыночной цене, но ордер при этом лимитный 
            await context.PlaceOrder(sellFairPrice, -quantity);

            //временные манипуляции с балансом, вообще он должен обновляеться 
            _balance += context.configuration.ContractValue / sellFairPrice;
        }
    }
}
