using Serilog;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TradeBot.TradeMarket.TradeMarketService.v1;

namespace Former
{
    public class Former
    {
        private readonly int _ordersCount;

        private readonly TradeMarketClient _tmClient;

        private readonly List<SubscribeOrdersResponse> _currentBuyOrders;

        public Former(int ordersCount)
        {
            _ordersCount = ordersCount;
            _tmClient = TradeMarketClient.GetInstance();
            _tmClient.NewOrder += NewOrder;

            _currentBuyOrders = new List<SubscribeOrdersResponse>();
        }

        private async void NewOrder(SubscribeOrdersResponse ordersResponse)
        {
            Log.Debug("Принял от маркета заказ {id}: цена {price}, количество {value}", ordersResponse.Response.Order.Id, ordersResponse.Response.Order.Price, ordersResponse.Response.Order.Quantity);

            var task = Task.Run(() =>
            {
                var index = _currentBuyOrders.FindIndex(x => x.Response.Order.Id == ordersResponse.Response.Order.Id);
                if (index != -1)
                {
                    _currentBuyOrders[index] = ordersResponse;
                    _currentBuyOrders.Sort(new ReplyComparator());
                }
                else
                {
                    if (_currentBuyOrders.Count >= _ordersCount)
                    {
                        _currentBuyOrders.RemoveAt(_ordersCount);
                        _currentBuyOrders.Add(ordersResponse);
                        _currentBuyOrders.Sort(new ReplyComparator());
                    }
                    else
                    {
                        _currentBuyOrders.Add(ordersResponse);
                        _currentBuyOrders.Sort(new ReplyComparator());
                    }
                }
            });

            await task;
        }

        public async void FormShoppingList(double avgPrice)
        {
            Log.Debug("Получено от алгоритма: {price}", avgPrice);

            var selectedOrders = _currentBuyOrders.Where(order => order.Response.Order.Price <= avgPrice);
            var shoppingList = selectedOrders.ToDictionary(order => order.Response.Order.Id, order => avgPrice);

            Log.Debug("Сформировал список необходимых ордеров: {elements}", shoppingList.ToArray());
            await _tmClient.SendShoppingList(shoppingList);
        }
    }
}
