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
        private int _pointIntervalInMilliseconds = 1000;
        public void SetPointInterval(int newInterval)
        {
            _pointIntervalInMilliseconds = newInterval;
        }
        private bool _isOrderedToStop = false;
        public void Stop()
        {
            _isOrderedToStop = true;
        }
        private KeyValuePair<DateTime, double> MakePoint(List<Order> orders, DateTime timestamp)
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

        public void Launch(PointPublisher publisher, DataCollector dataCollector)
        {
            while (!_isOrderedToStop)
            {
                Log.Information("New iteration started");
                Thread.Sleep(_pointIntervalInMilliseconds);
                if (dataCollector.Orders.Count == 0)
                    continue;

                var newOrders = dataCollector.Orders;
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
