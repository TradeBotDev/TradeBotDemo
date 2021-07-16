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

        private readonly List<SubscribeOrdersResponse> _currentBuyOrders;

        public Former()
        {
            _tmClient = TradeMarketClient.GetInstance();
            _currentBuyOrders = new List<SubscribeOrdersResponse>();
        }

        public async void FormShoppingList(double avgPrice)
        {
            Log.Debug("Получено от алгоритма: {price}", avgPrice);

            var selectedOrders = _currentBuyOrders.Where(order => order.Response.Order.Price <= avgPrice);

            var shoppingList = selectedOrders.ToDictionary(order => order.Response.Order.Id, order => avgPrice);

            Log.Debug("Сформировал список необходимых ордеров: {elements}", shoppingList.ToArray());
            await _tmClient.SendShoppingList(shoppingList);
        }

        public async void UpdateCurrentOrders(SubscribeOrdersResponse orderNeededUpdate)
        {
            Log.Debug("Принял от маркета заказ {id}: цена {price}, количество {value}", orderNeededUpdate.Response.Order.Id, orderNeededUpdate.Response.Order.Price, orderNeededUpdate.Response.Order.Quantity);

            var task = Task.Run(() =>
            {
                if (_currentBuyOrders.FindAll(x => x.Response.Order.Id == orderNeededUpdate.Response.Order.Id).Count != 0)
                {
                    var updatedIndex = _currentBuyOrders.FindIndex(x => x.Response.Order.Id == orderNeededUpdate.Response.Order.Id);
                    _currentBuyOrders.RemoveAt(updatedIndex);
                    _currentBuyOrders.Insert(updatedIndex, orderNeededUpdate);
                    Array.Sort(_currentBuyOrders.ToArray(), new ReplyComparator());
                }
                else
                {
                    //TODO разобраться с размером | стакана цен
                    //                            V                              
                    if (_currentBuyOrders.Count == OrdersCount)
                    {
                        Array.Sort(_currentBuyOrders.ToArray(), new ReplyComparator());
                        _currentBuyOrders.RemoveAt(OrdersCount);
                        _currentBuyOrders.Add(orderNeededUpdate);
                        Array.Sort(_currentBuyOrders.ToArray(), new ReplyComparator());
                    }
                    else
                    {
                        _currentBuyOrders.Add(orderNeededUpdate);
                        Array.Sort(_currentBuyOrders.ToArray(), new ReplyComparator());
                    }
                }
            });
            await task;
        }

        public class ReplyComparator : IComparer<SubscribeOrdersResponse>
        {
            int IComparer<SubscribeOrdersResponse>.Compare(SubscribeOrdersResponse x, SubscribeOrdersResponse y)
            {
                return x.Response.Order.Price.CompareTo(y.Response.Order.Price);
            }
        }
    }
}
