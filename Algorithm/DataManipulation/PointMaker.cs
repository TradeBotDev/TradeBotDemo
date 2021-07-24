using System;
using System.Collections.Generic;
using System.Threading;

using TradeBot.Common.v1;

namespace Algorithm.DataManipulation
{
    //hardcoded for three second time intervals between points for now, 
    //later will be decided by user
    public class PointMaker
    {
        private static  KeyValuePair<DateTime, double> MakePoint(List<Order> orders, DateTime timestamp)
        {           
            double price = 0;
            int numberOfOrders = 0;

            foreach (Order order in orders)
            {
                if (order.Price != 0)
                {
                    price += order.Price;
                    numberOfOrders++;
                }
            }
            if (numberOfOrders != 0)
            {
                price /= numberOfOrders;
            } else { price = 0; }
            Console.WriteLine("Made a point: " + timestamp.TimeOfDay + "    " + price);
            return new KeyValuePair<DateTime, double>(timestamp, price);
        }

        public void Launch(Publisher publisher, DataCollector dataCollector)
        {
            while (true)
            {
                Thread.Sleep(2000);
                if (DataCollector.Orders.Count == 0)
                    continue;

                    var newOrders = DataCollector.Orders;
                    var newPoint = MakePoint(new List<Order>(newOrders), DateTime.Now);
                    publisher.Publish(newPoint);                                 
            }
        }       
    }
}
