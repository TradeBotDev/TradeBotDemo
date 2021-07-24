using Algorithm.DataManipulation;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorithm.Analysis
{
    //public delegate void SendPrice (double price);
    
    //if the trend has been descending for three points and is now on the rise we're buying
    //hardcoded for five points for now, will change with user config
    public class AlgorithmAlpha
    {
        private readonly Dictionary<DateTime, double> _storage;

        public AlgorithmAlpha(Publisher publisher)
        {

            publisher.PointMadeEvent += NewPointAlert;

            _storage = new Dictionary<DateTime, double>();
        }

        //hardcoded for five points!! 
        private void NewPointAlert(KeyValuePair<DateTime, double> point)
        {
            // break here?? 
            if (point.Value != 0)
            { 
                _storage.Add(point.Key, point.Value); 
            }
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
        
        private static void PerformCalculations (Dictionary<DateTime, double> points)
        {
            points.OrderBy(kvp => kvp.Key);
            List<double> prices = new();
            Dictionary<DateTime, double> subSet = new();

            for (int i = 0; i < 3; i++)
            {
                subSet.Clear();
                subSet.Add(points.ElementAt(i).Key, points.ElementAt(i).Value);
                subSet.Add(points.ElementAt(i + 1).Key, points.ElementAt(i + 1).Value);
                subSet.Add(points.ElementAt(i + 2).Key, points.ElementAt(i + 2).Value);
                prices.Add(CalculateSMA(subSet));
            }
            // TODO alert the price sender!!
            if (IsItTimeToBuy(prices, points)) 
            { 
                PriceSender.SendPrice(prices.Last()); 
            }
        }
        //if the trend has been going downwards and now stopped and it going up
        public static bool IsItTimeToBuy(IReadOnlyCollection<double> prices, Dictionary<DateTime, double> points)
        {
            points.OrderBy(kvp => kvp.Key);
            Console.WriteLine("Analysis...");
            return prices.ElementAt(0) >= prices.ElementAt(1)
                   && prices.ElementAt(1) >= prices.ElementAt(2)
                   && prices.ElementAt(2) <= points.ElementAt(4).Value;
        }
        public static double CalculateSMA(Dictionary<DateTime, double> points)
        {
            var sum = points.Sum(point => point.Value);
            return sum / points.Count;

        }
    }
}
