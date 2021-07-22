using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using System.Threading;

namespace Algorithm.DataManipulation
{
    //hardcoded for three second time intervals between points for now, 
    //later will be decided by user
    public class PointMaker
    {
        public event EventHandler<KeyValuePair<DateTime, double>> PointMadeEvent;
        public KeyValuePair<DateTime, double> MakePoint(List<Order> orders, DateTime timestamp)
        {           
            double price = 0;

            foreach (Order order in orders)
            {
                price += order.Price;
            }
            price /= orders.Count;
            Console.WriteLine("Made a point: " + timestamp.TimeOfDay + " " + price);
            return new KeyValuePair<DateTime, double> (timestamp, price); 
        }

        public void Launch(Publisher publisher, DataCollector dataCollector)
        {
            List<Order> newOrders = new();

            while (true)
            {
                Thread.Sleep(3000);
                if (DataCollector.orders.Count != 0)
                {
                    newOrders = DataCollector.orders;
                    KeyValuePair<DateTime, double> newPoint = MakePoint(newOrders, DateTime.Now);
                    publisher.Publish(newPoint);
                    newOrders.Clear();
                }
            }
        }

        

    }
}
