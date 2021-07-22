using Algorithm.DataManipulation;

using System;
using System.Collections.Generic;
using System.Linq;
using TradeBot.Common.v1;

namespace Algorithm.DataManipulation
{
    public class DataCollector
    {
        public static List<Order> Orders;

        private readonly Publisher _publisher = new();

        public DataCollector()
        {
            Orders = new List<Order>();
            _publisher.PointMadeEvent += ClearUsedData;
        }

        private static void ClearUsedData (KeyValuePair<DateTime, double> point)
        {
            foreach(var order in Orders.Where(order => DateTime.Compare(order.LastUpdateDate.ToDateTime(), point.Key) < 0))
            {
                Orders.Remove(order);
            }
        }

    }
}
