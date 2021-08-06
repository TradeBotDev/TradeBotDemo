using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;

using TradeBot.Common.v1;

namespace Algorithm.DataManipulation
{
    //PM takes incoming orders from DataCollector and makes average points with regular time intervals to simplify the data and make it digestable for the algo to calculate stuff with 
    //hardcoded for one second time intervals between points for now
    //later will be decided by user
    public class PointMaker
    {
        private static KeyValuePair<DateTime, double> MakePoint(List<Order> orders, DateTime timestamp)
        {
            double price = 0;
            int numberOfOrders = 0;

            foreach (Order order in orders)
            {
                //sometimes it recieves an order with a price zero (for example, if the order in question was updated)
                //they shouldn't be used for calculations so we ignore them  
                if (order.Price != 0)
                {
                    price += order.Price;
                    numberOfOrders++;
                }
            }
            //sometimes we don't recieve anything from Relay within the timeframe (no new orders)
            //if that happens we set the price to zero to help algo understand that that has happened 
            if (numberOfOrders != 0)
            {
                price /= numberOfOrders;
                Log.Information("Made a point: " + timestamp.TimeOfDay + "    " + price);
            }
            else
            {
                price = 0;
                Log.Information("No new orders were recieved");
            }

            return new KeyValuePair<DateTime, double>(timestamp, price);
        }

        public void Launch(Publisher publisher, DataCollector dataCollector)
        {
            while (true)
            {
                Log.Information("New iteration started");
                Thread.Sleep(1000);
                if (DataCollector.Orders.Count == 0)
                    continue;

                var newOrders = DataCollector.Orders;
                var newPoint = MakePoint(new List<Order>(newOrders), DateTime.Now);
                //if a point was made we notify everyone involved (algo and dataCollector) 
                if (newPoint.Value != 0)
                {
                    publisher.Publish(newPoint);
                }
            }
        }
    }
}
