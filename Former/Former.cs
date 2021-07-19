using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TradeBot.TradeMarket.TradeMarketService.v1;

namespace Former
{
    public class Former
    {
        public TradeBot.Common.v1.Config Config;

        private const int OrdersCount = 9;

        private readonly TradeMarketClient _tmClient;

        private readonly SortedList<double, SubscribeOrdersResponse> _currentBuyOrders;

        public Former()
        {
            _tmClient = TradeMarketClient.GetInstance();
            _currentBuyOrders = new SortedList<double, SubscribeOrdersResponse>();
        }

        //public async void FormShoppingList(double avgPrice)
        //{
        //    Log.Debug("Получено от алгоритма: {price}", avgPrice);

        //    var selectedOrders = _currentBuyOrders.Where(order => order.Key <= avgPrice);

        //    var shoppingList = selectedOrders.ToDictionary(order => order.Response.Order.Id, order => avgPrice);

        //    Log.Debug("Сформировал список необходимых ордеров: {elements}", shoppingList.ToArray());
        //    await _tmClient.SendShoppingList(shoppingList);
        //}

        //public async void UpdateCurrentOrders(SubscribeOrdersResponse orderNeededUpdate)
        //{
        //    Log.Debug("Принял от маркета заказ {id}: цена {price}, количество {value}", orderNeededUpdate.Response.Order.Id, orderNeededUpdate.Response.Order.Price, orderNeededUpdate.Response.Order.Quantity);

        //    var task = Task.Run(() =>
        //    {
        //        if (orderNeededUpdate.)
        //    });
        //    await task;
        //}

        public class ReplyComparator : IComparer<SubscribeOrdersResponse>
        {
            int IComparer<SubscribeOrdersResponse>.Compare(SubscribeOrdersResponse x, SubscribeOrdersResponse y)
            {
                return x.Response.Order.Price.CompareTo(y.Response.Order.Price);
            }
        }
    }
}
