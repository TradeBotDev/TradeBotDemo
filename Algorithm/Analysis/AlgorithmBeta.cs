using Algorithm.DataManipulation;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorithm.Analysis
{
    public class AlgorithmBeta
    {
        //algo's inner storage of points made by PointMaker
        private readonly Dictionary<DateTime, double> _storage;

        //when an algo is created it's immediately subsribed to new points 
        public AlgorithmBeta(Publisher publisher)
        {
            publisher.PointMadeEvent += NewPointAlert;
            _storage = new Dictionary<DateTime, double>();
        }

        //when a new point is made algo adds it to its storage and checks if it has enough to initiate analysis 
        //hardcoded for five points!! 
        private void NewPointAlert(KeyValuePair<DateTime, double> point)
        {
            
        }
        //func to find average price of a dict (used so that we don't have to iterate the dict in the other funcs)
        public static double CalculateSMA(Dictionary<DateTime, double> points)
        {
            if (points.Count == 0)
            {
                throw new ArgumentException("Cannot calculate an average on an empty dictionary");
            }
            var sum = points.Sum(point => point.Value);
            return sum / points.Count;
        }

        public static List<double> CalculateEMA(List<double> prices)
        {
            if (prices.Count()==1)
            {
                return new List<double>(){ prices.ElementAt(0) };
            }
            if (prices.Count()==0)
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

            foreach(var price in prices)
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
