using Algorithm.DataManipulation;

using System;
using System.Collections.Generic;
using System.Linq;
using TradeBot.Common.v1;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Grpc.Core;
using Serilog;
using Algorithm.Services;

namespace Algorithm.DataManipulation
{
    //DataCollector receives orders from the Relay service
    //it is basically a data storage to use instead of a DB 
    public class DataCollector
    {
        //all the orders coming from Relay
        public BlockingCollection<OrderWrapper> Orders;

        public DataCollector(PointPublisher publisher)
        {
            Orders = new BlockingCollection<OrderWrapper>();
            //if PointMaker had taken the orders and made a point we can clear the storage
            publisher.PointMadeEvent += ClearUsedData;
        }

        public void ClearUsedData (KeyValuePair<DateTime, double> point)
        {
                foreach (var order in Orders.Where(order => DateTime.Compare(order.LastUpdateDate, point.Key) < 0))
                {
                    Orders.Take();
                }
        }

        public void AddNewOrder(OrderWrapper order)
        {
            Orders.Add(order);
        }

        public void ClearAllData()
        {
            foreach (OrderWrapper order in Orders)
            {
                Orders.Take();
            }
        }
    }
}
