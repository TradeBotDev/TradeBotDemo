using Algorithm.DataManipulation;
using Algorithm.Services;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.Former.FormerService.v1;

namespace Algorithm
{
    public class DataCollector
    {
        public static List<Order> orders;
        Publisher publisher = new Publisher();

        public DataCollector()
        {
            orders = new();
            publisher.pointMadeEvent += ClearUsedData;
        }

        private void ClearUsedData (KeyValuePair<DateTime, double> point)
        {
            foreach(Order order in orders)
            {
                if ((DateTime.Compare(order.LastUpdateDate.ToDateTime(), point.Key))<0) { orders.Remove(order); }
            }
        }

    }
}
