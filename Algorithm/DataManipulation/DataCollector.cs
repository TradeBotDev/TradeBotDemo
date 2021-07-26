using Algorithm.DataManipulation;

using System;
using System.Collections.Generic;
using System.Linq;
using TradeBot.Common.v1;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Grpc.Core;

namespace Algorithm.DataManipulation
{
    public class DataCollector
    {
        public static BlockingCollection<Order> Orders;
        public static Grpc.Core.Metadata metaData;

        //private readonly Publisher _publisher = new();


        public DataCollector(Publisher publisher)
        {
            Orders = new BlockingCollection<Order>();
            //_publisher.PointMadeEvent += ClearUsedData;
            publisher.PointMadeEvent += ClearUsedData;
        }

        public void ClearUsedData (KeyValuePair<DateTime, double> point)
        {
                foreach (var order in Orders.Where(order => DateTime.Compare(order.LastUpdateDate.ToDateTime(), point.Key) < 0))
                {
                    Orders.Take();
                }
        }
    }
}
