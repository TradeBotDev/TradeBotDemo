using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeMarket.Former;

namespace Former
{

    public class Former
    {
        private static List<SubscribeOrdersReply> CurrentBuyOrders = new();
        private static List<string> ShoppingList = new();

        public static void FormShoppingList(double AvgPrice)
        {
            ShoppingList.Clear();
            Console.WriteLine("Получено от алгоритма: " + AvgPrice);
            foreach (var order in CurrentBuyOrders)
            {
                if (order.SimpleOrderInfo.Price <= AvgPrice) ShoppingList.Add(order.Id);
            }
            Console.Write("\nСформировал список необходимых ордеров: \n{ ");
            foreach (var elem in ShoppingList)
            {
                Console.Write(elem);
                Console.Write(", ");
            }
            Console.WriteLine("\b\b }");
        }

        public static async void UpdateCurrentOrders(SubscribeOrdersReply orderNeededUpdate)
        {
            Console.WriteLine("Принял от маркета заказ {0}: цена {1}, количество {2}", orderNeededUpdate.Id, orderNeededUpdate.SimpleOrderInfo.Price, orderNeededUpdate.SimpleOrderInfo.Quantity);
            var task = Task.Run(() =>
            {
                if (CurrentBuyOrders.FindAll(x => x.Id == orderNeededUpdate.Id).Count != 0)
                {
                    int updatedIndex = CurrentBuyOrders.FindIndex(x => x.Id == orderNeededUpdate.Id);
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

        public class ReplyComparator : IComparer<SubscribeOrdersReply>
        {
            int IComparer<SubscribeOrdersReply>.Compare(SubscribeOrdersReply x, SubscribeOrdersReply y)
            {
                return x.SimpleOrderInfo.Price.CompareTo(y.SimpleOrderInfo.Price);
            }
        }
    }
}
