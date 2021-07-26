using Algorithm.DataManipulation;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorithm.Analysis
{
    //if the trend has been descending for three points and is now on the rise we're buying
    //hardcoded for five points for now, will change with user config
    //the name Alpha suggests it's the most basic algo
    //HUGE TODO #1 lots of hardcoded stuff here, all needs to be remade
    //HUGE TODO #1.5 it needs to take user input and change accordingly 
    //HUGE TODO #2 this algo uses SMA, we potentially want to move to EMA 
    //HUGE TODO #3 think about history analysis, because atm we only do real-time calculations
    public class AlgorithmAlpha
    {
        //algo's inner storage of points made by PointMaker
        private readonly Dictionary<DateTime, double> _storage;

        //when an algo is created it's immediately subsribed to new points 
        public AlgorithmAlpha(Publisher publisher)
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
        private static void PerformCalculations (Dictionary<DateTime, double> points)
        {
            points.OrderBy(kvp => kvp.Key);
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
            if (IsItTimeToBuy(prices, points)) 
            { 
                PriceSender.SendPrice(points.Last().Value); 
            }
        }
        //this func needs points and subset averages and decides if it's time to buy
        public static bool IsItTimeToBuy(IReadOnlyCollection<double> prices, Dictionary<DateTime, double> points)
        {
            Console.WriteLine("Analysis...");

            //if our averages are getting lower with each iteration we conclude the marker price is generally falling
            if (prices.ElementAt(0) >= prices.ElementAt(1) && prices.ElementAt(1) >= prices.ElementAt(2))
            {
                Console.WriteLine("Downward trend detected");
                //if the latest price is higher (but not too much, not over 15%) we think the price might start rising
                //the 15% is need to avoid accidentally buying on a sudden spike 
                //this means now is the time to buy
                return prices.ElementAt(2) <= points.ElementAt(4).Value &&
                    prices.ElementAt(2)*1.15 > points.ElementAt(4).Value;
            }
            return false;
        }
        //func to find average price of a dict (used so that we don't have to iterate the dict in the other funcs)
        public static double CalculateSMA(Dictionary<DateTime, double> points)
        {
            var sum = points.Sum(point => point.Value);
            return sum / points.Count;

        }
    }
}
