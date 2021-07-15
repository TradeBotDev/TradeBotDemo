using Former.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeBot.Former.FormerService.v1;
using TradeBot.Common.v1;

namespace Former
{
    public class Former
    {
        private static List<TradeBot.Common.v1.SubscribeOrdersResponse> CurrentBuyOrders = new();
        private static List<string> ShoppingList = new();
        public static Config config;

        public async static void FormShoppingList(double AvgPrice)
        {
            ShoppingList.Clear();
            Console.WriteLine("Получено от алгоритма: " + AvgPrice);
            foreach (var order in CurrentBuyOrders)
            {
                if (order.Response.Price <= AvgPrice) ShoppingList.Add(order.Response.Id);
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

        public static async void UpdateCurrentOrders(TradeBot.Common.v1.SubscribeOrdersResponse orderNeededUpdate)
        {
            Console.WriteLine("Принял от маркета заказ {0}: цена {1}, количество {2}", orderNeededUpdate.Response.Id, orderNeededUpdate.Response.Price, orderNeededUpdate.Response.Quantity);
            var task = Task.Run(() =>
            {
                if (CurrentBuyOrders.FindAll(x => x.Response.Id == orderNeededUpdate.Response.Id).Count != 0)
                {
                    int updatedIndex = CurrentBuyOrders.FindIndex(x => x.Response.Id == orderNeededUpdate.Response.Id);
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
                return x.Response.Price.CompareTo(y.Response.Price);
            }
        }
    }
}
