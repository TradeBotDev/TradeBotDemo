using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Algorithm.Analysis
{
    public static class MarketAnalyser
    {
        //Market analyser initiates some calculations when the program is started 
        //it outputs a general trend looking back some time 

        //
        private static Dictionary<DateTime, double> _storage;

        //func to find average price of a dict 
        private static double CalculateSMA(Dictionary<DateTime, double> points)
        {
            var sum = points.Sum(point => point.Value);
            return sum / points.Count;
        }
        private static List<double> CalculateEMA(List<double> prices)
        {
            if (prices.Count() == 1)
            {
                return new List<double>() { prices.ElementAt(0) };
            }
            if (prices.Count() == 0)
            {
                return new List<double>();
            }
            List<double> EMAs = new();
            double currentEMA = 0;
            //the first element's EMA is always equal to its SMA
            double previousEMA = prices.ElementAt(0);
            EMAs.Add(previousEMA);
            prices.RemoveAt(0);
            //counter needed for the formula, for more info look further down
            int i = 3;

            foreach (var price in prices)
            {
                //the formula is EMA = Price(current) * K + EMA(previous) * (1-K)
                //K = 2/(N+1), where N - amount of points accounted for
                currentEMA = price * 2 / i + previousEMA - previousEMA * 2 / i;
                EMAs.Add(currentEMA);
                previousEMA = currentEMA;
                i++;
            }
            return EMAs;
        }
    }
}
