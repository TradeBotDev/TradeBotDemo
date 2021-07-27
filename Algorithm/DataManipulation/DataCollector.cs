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
    //DataCollector receives orders from the Relay service
    //it is basically a data storage to use instead of a DB 
    //maybe TODO replace it w/ a DB 
    public class DataCollector
    {
        //all the orders coming from Relay
        public static BlockingCollection<Order> Orders;
        //metadata coming w/ orders (only needed to be relayed to Former for now)
        public static Grpc.Core.Metadata metaData;

        public DataCollector(Publisher publisher)
        {
            Orders = new BlockingCollection<Order>();
            //if PointMaker had taken the orders and made a point we can clear the storage
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
