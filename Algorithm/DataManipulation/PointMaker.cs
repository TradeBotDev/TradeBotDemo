using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Algorithm.DataManipulation
{
    public class PointMaker : IPointRepository
    {
        public bool initialAnalysisDone = false;
        public Dictionary<DateTime, double> GetPoints(string SlotID)
        {
            Dictionary<DateTime, double> requestedPoints = new Dictionary<DateTime, double>();
            return requestedPoints;
        }

        public KeyValuePair<DateTime, double> MakePoint(List<Order> orders, DateTime timestamp)
        {           
            double price = 0;

            foreach (Order order in orders)
            {
                price += order.Price;
            }
            price = price / orders.Count();

            return new KeyValuePair<DateTime, double> (timestamp, price); 
        }

        public double CalculateEMA (Dictionary<DateTime, double> points)
        {
            SortedDictionary<DateTime, double> sortedPoints = new SortedDictionary<DateTime, double>(points);


        }
    }
}
