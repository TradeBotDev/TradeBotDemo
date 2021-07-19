using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using SubscribeOrdersResponse = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersResponse;

namespace Former
{
    class Balances
    {
        public double bal1 = 0;
        public double bal2 = 0;
    }

    public class Former
    {
        public TradeBot.Common.v1.Config Config;

        private const int OrdersCount = 9;

        private readonly TradeMarketClient _tmClient;

        private readonly List<SubscribeOrdersResponse> _currentPurchaseOrders;

        private readonly List</*cyka*/> _myOrders;

        private readonly Balances balances;

        public Former()
        {
            _tmClient = TradeMarketClient.GetInstance();
            _currentPurchaseOrders = new List<SubscribeOrdersResponse>();
        }

        public async void FormShoppingList(double avgPrice)
        {
            Log.Debug("Получено от алгоритма: {price}", avgPrice);

            var selectedOrders = _actualPurchaseOrders.Where(order => order.Response.Order.Price <= avgPrice);

            var shoppingList = selectedOrders.ToDictionary(order => order.Response.Order.Id, order => avgPrice);

            Log.Debug("Сформировал список необходимых ордеров: {elements}", shoppingList.ToArray());
            await _tmClient.SendShoppingList(shoppingList);
        }

        public async void UpdateCurrentOrders(SubscribeOrdersResponse orderNeededUpdate)
        {
            Log.Debug("Принял от маркета заказ {id}: цена {price}, количество {value}", orderNeededUpdate.Response.Order.Id, orderNeededUpdate.Response.Order.Price, orderNeededUpdate.Response.Order.Quantity);
            var task = Task.Run(() =>
            {
                int index = _currentPurchaseOrder_currentPurchaseOrders_currentPurchaseOrders_currentPurchaseOrders_currentPurchaseOrders_currentPurchaseOrders_currentPurchaseOrders_currentPurchaseOrders_currentPurchaseOrders_currentPurchaseOrders_currentPurchaseOrders_currentPurchaseOrders_currentPurchaseOrders_currentPurchaseOrders_currentPurchaseOrders_currentPurchaseOrderss.FindIndex(x => x.Response.Order.Id == orderNeededUpdate.Response.Order.Id);
                if (orderNeededUpdate.Response.Order.Signature.Status == OrderStatus.Open)
                    if (_actualPurchaseOrders.Exists(x => x.Response.Order.Id == orderNeededUpdate.Response.Order.Id))
                        _actualPurchaseOrders[index] = orderNeededUpdate;
                    else _actualPurchaseOrders.Add(orderNeededUpdate);
                else _actualPurchaseOrders.RemoveAt(index);

                _actualPurchaseOrders.Sort(new ReplyComparator());
            });
            await task;
        }

        public void UpdateCurrentBalance(string bal1, string bal2)
        {
            balances.bal1 = double.Parse(bal1);
            balances.bal2 = double.Parse(bal2);
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
