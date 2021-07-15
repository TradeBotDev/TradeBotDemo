using Former.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeBot.TradeMarket.TradeMarketService.v1;

namespace Former
{
    public class Former
    {
        private static List<SubscribeOrdersResponse> CurrentBuyOrders = new();
        public static TradeBot.Common.v1.Config config;
        private static Dictionary<string, double> ShoppingList = new();

        public static async void FormShoppingList(double AvgPrice)
        {
            ShoppingList.Clear();
            Console.WriteLine("Получено от алгоритма: " + AvgPrice);
            foreach (var order in CurrentBuyOrders)
            {
                if (order.Response.Order.Price <= AvgPrice) ShoppingList.Add(order.Response.Order.Id, AvgPrice + config.SlotFee + config.RequiredProfit);
            }
            Console.Write("\nСформировал список необходимых ордеров: \n{ ");
            foreach (var elem in ShoppingList)
            {
                Console.Write(elem);
                Console.Write(", ");
            }
            Console.WriteLine("\b\b }");
            await Observer.SendShopingList(ShoppingList);
        }

        public static async void UpdateCurrentOrders(SubscribeOrdersResponse orderNeededUpdate)
        {
            Console.WriteLine("Принял от маркета заказ {0}: цена {1}, количество {2}", orderNeededUpdate.Response.Order.Id, orderNeededUpdate.Response.Order.Price, orderNeededUpdate.Response.Order.Quantity);
            var task = Task.Run(() =>
            {
                if (CurrentBuyOrders.FindAll(x => x.Response.Order.Id == orderNeededUpdate.Response.Order.Id).Count != 0)
                {
                    int updatedIndex = CurrentBuyOrders.FindIndex(x => x.Response.Order.Id == orderNeededUpdate.Response.Order.Id);
                    CurrentBuyOrders.RemoveAt(updatedIndex);
                    CurrentBuyOrders.Insert(updatedIndex, orderNeededUpdate);
                    Array.Sort(CurrentBuyOrders.ToArray(), new ReplyComparator());
                }
                else
                {
                    //TODO разобраться с размером | стакана цен
                    //                            V                              
                    if (CurrentBuyOrders.Count == 9)
                    {
                        Array.Sort(CurrentBuyOrders.ToArray(), new ReplyComparator());
                        CurrentBuyOrders.RemoveAt(9);
                        CurrentBuyOrders.Add(orderNeededUpdate);
                        Array.Sort(CurrentBuyOrders.ToArray(), new ReplyComparator());
                    }
                    else
                    {
                        CurrentBuyOrders.Add(orderNeededUpdate);
                        Array.Sort(CurrentBuyOrders.ToArray(), new ReplyComparator());
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
