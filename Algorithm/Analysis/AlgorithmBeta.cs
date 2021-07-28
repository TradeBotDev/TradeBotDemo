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
            if (point.Value != 0)
            {
                _storage.Add(point.Key, point.Value);

                if (_storage.Count > 5)
                {
                    var toRemove = _storage.OrderBy(kvp => kvp.Key).First();
                    _storage.Remove(toRemove.Key);
                }
                if (_storage.Count == 5)
                {
                    PerformCalculations(_storage);
                }
            }
        }

        //this func prepares the data and averages to pass into analysing funcs 
        private static void PerformCalculations(Dictionary<DateTime, double> points)
        {
            List<double> prices = new();
            Dictionary<DateTime, double> subSet = new();

            //creates three (hardcoded) subsequent subsets and gets an average of each (this is needed to find the general price trend)
            for (int i = 0; i < 3; i++)
            {
                subSet.Clear();
                subSet.Add(points.ElementAt(i).Key, points.ElementAt(i).Value);
                subSet.Add(points.ElementAt(i + 1).Key, points.ElementAt(i + 1).Value);
                subSet.Add(points.ElementAt(i + 2).Key, points.ElementAt(i + 2).Value);
                prices.Add(CalculateSMA(subSet));
            }
            //after all the data is prepared the actual analysis takes place
            int trend = TrendAnalyser(prices, points);
            if (trend != 0)
            {
                PriceSender.SendPrice(trend);
            }
        }
        //this func needs points and subset averages and decides if it's time to buy
        private static int TrendAnalyser(IReadOnlyCollection<double> averages, Dictionary<DateTime, double> points)
        {
            Log.Information("Analysis...");

            //if our averages are getting lower with each iteration we conclude the marker price is generally falling
            if (averages.ElementAt(0) >= averages.ElementAt(1) && averages.ElementAt(1) >= averages.ElementAt(2))
            {
                Log.Information("Downward trend detected");
                //if the latest price is higher (but not too much, not over 15%) we think the price might start rising
                //the 15% is needed to avoid accidentally buying on a sudden spike 

                if (averages.ElementAt(2) <= points.ElementAt(4).Value &&
                    averages.ElementAt(2) * 1.15 > points.ElementAt(4).Value)
                {
                    return 1;
                }
            }

            if (averages.ElementAt(0) <= averages.ElementAt(1) && averages.ElementAt(1) <= averages.ElementAt(2))
            {
                Log.Information("Upward trend detected");

                if (averages.ElementAt(2) >= points.ElementAt(4).Value &&
                    averages.ElementAt(2) < points.ElementAt(4).Value * 1.15)
                {
                    return -1;
                }
            }
            return 0;

        }
        //func to find average price of a dict (used so that we don't have to iterate the dict in the other funcs)
        private static double CalculateSMA(Dictionary<DateTime, double> points)
        {
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
