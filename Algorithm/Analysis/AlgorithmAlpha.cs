using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Algorithm.Analysis
{
    //if the trend has been descending for three points and is now on the rise we're buying
    //hardcoded for five points for now, will change with user config
    public class AlgorithmAlpha
    {
        private Dictionary<DateTime, double> storage;
        
        //hardcoded for five points!! 
        public void NewPointAlert (KeyValuePair<DateTime, double> point)
        {
            storage.Add(point.Key, point.Value);
            if (storage.Count > 5)
            {
                var toRemove = storage.OrderBy(kvp => kvp.Key).First();
                storage.Remove(toRemove.Key);
            }
            if (storage.Count == 5) { PerformCalculations(storage); }

        }
        
        public void PerformCalculations (Dictionary<DateTime, double> points)
        {
            points.OrderBy(kvp => kvp.Key);
            List<double> prices = new();
            Dictionary<DateTime, double> subSet = new();

            for (int i = 0; i < 3; i++)
            {
                subSet.Clear();
                subSet.Add(points.ElementAt(i).Key, points.ElementAt(i).Value);
                subSet.Add(points.ElementAt(i+1).Key, points.ElementAt(i+1).Value);
                subSet.Add(points.ElementAt(i+2).Key, points.ElementAt(i+2).Value);
                prices.Add(CalculateSMA(subSet));
            }
            // TODO alert the price sender!!
            if (IsItTimeToBuy(prices, points)) { }
        }
        //if the trend has been going downwards and now stopped and it going up
        public bool IsItTimeToBuy(List<double> prices, Dictionary<DateTime, double> points)
        {
            points.OrderBy(kvp => kvp.Key);
            if ((prices.ElementAt(0) >= prices.ElementAt(1))&&(prices.ElementAt(1) >= prices.ElementAt(2))&&(prices.ElementAt(2)<points.ElementAt(4).Value)) { return true; } 
            return false;
        }
        public double CalculateSMA(Dictionary<DateTime, double> points)
        {
            double sum = 0;
            foreach (KeyValuePair<DateTime, double> point in points)
            {
                sum += point.Value;
            }

            return sum / points.Count;

        }
    }
}
