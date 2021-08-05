using Algorithm.DataManipulation;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorithm.Analysis
{
    public class AlgorithmBeta
    {
        //This algo looks back several seconds (set by user) and looks for price peaks and pits 
        //we analyse the trend by calculating SMAs (simple moving average) of several subsets withing the timeframe
        //if the general price trend is rising/falling, but the current price is doing the opposite we conclude that this might be a local pit or a peak
        //pits are good for buying (lowest price we could find)
        //peaks are good for selling (highest price we could find)
        //this decision is then sent to Former service which does what it can with it 

        //algo's inner storage of points made by PointMaker
        //incoming into Algorithm service are all the orders from TM, so in order to simplify this data for calculations we transform orders into "point"
        //these points are basically the average market prices at some specific points in time
        //we make a point every second if possible
        private readonly Dictionary<DateTime, double> _storage;
        //user sets how long into the past the algo analyses
        //each second = one point
        private readonly int _durationInSeconds = 5;

        //when an algo is created it's immediately subsribed to new points 
        public AlgorithmBeta(Publisher publisher)
        {
            publisher.PointMadeEvent += NewPointAlert;
            _storage = new Dictionary<DateTime, double>();
        }

        //when a new point is made algo adds it to its storage and checks if it has enough to initiate analysis 
        //if the storage is overfilled we remove the oldest point
        private void NewPointAlert(KeyValuePair<DateTime, double> point)
        {
            if (point.Value != 0)
            {
                _storage.Add(point.Key, point.Value);

                if (_storage.Count > _durationInSeconds)
                {
                    var toRemove = _storage.First();
                    _storage.Remove(toRemove.Key);
                }
                if (_storage.Count == _durationInSeconds)
                {
                    PerformCalculations(_storage);
                }
            }
        }

        //this function gathers all the data and sends it for analysis
        //
        private static void PerformCalculations(Dictionary<DateTime, double> points)
        {
            //subsets are used later in this func to help determine the general trend
            List<double> subSet = new();
            List<double> subTrends = new();

            //number of subsets is the total length of analysis divided by two and rounded up
            //this is just how it is
            int lengthOfSubsets = (int)Math.Ceiling((decimal)points.Count / 2);
            //again, that's just the algo formulas, no logic behind this
            int numberOfSubsets = points.Count + 1 - lengthOfSubsets;

            //creates several subsequent subsets and gets an average of each
            for (int i = 0; i < numberOfSubsets; i++)
            {
                subSet.Clear();
                for (int j = 0; j < lengthOfSubsets; j++)
                {
                    subSet.Add(points.ElementAt(j + i).Value);
                }
                subTrends.Add(CalculateSMA(subSet));
            }
            //after all the data is prepared the actual analysis takes place
            int trend = TrendAnalyser(subTrends, points);
            if (trend != 0)
            {
                PriceSender.SendPrice(trend);
            }
        }
        //this func needs points and subset averages and decides if it's time to buy
        private static int TrendAnalyser(IReadOnlyCollection<double> subTrends, Dictionary<DateTime, double> points)
        {
            Log.Information("Analysis...");
            bool downtrend = true;
            bool uptrend = true;

            //if our averages are getting lower with each iteration we conclude the market price is falling
            for (int i = 1; i < subTrends.Count; i++)
            {
                //if any of the subset averages are bigger than the previous we assume the trend is NOT downward
                if (subTrends.ElementAt(i-1) < subTrends.ElementAt(i))
                {
                    downtrend = false;
                    break;
                }
            }
            if (downtrend)
            {
                Log.Information("Downward trend detected");
                //if the latest price is higher (but not too much, not over 15%) we think the price might start rising
                //the 15% is needed to avoid accidentally buying on a sudden spike 

                if (subTrends.Last() <= points.Last().Value &&
                    subTrends.Last() * 1.15 > points.Last().Value)
                {
                    return 1;
                }
            }

            for (int i = 1; i < subTrends.Count; i++)
            {
                //if any of the subset averages are smaller than the previous we assume the trend is NOT upward
                if (subTrends.ElementAt(i - 1) > subTrends.ElementAt(i))
                {
                    uptrend = false;
                    break;
                }
            }

            if (uptrend)
            {
                Log.Information("Upward trend detected");

                if (subTrends.Last() >= points.Last().Value &&
                    subTrends.Last() < points.Last().Value * 1.15)
                {
                    return -1;
                }
            }
            return 0;
        }
        //func to find average price
        private static double CalculateSMA(List<double> points)
        {
            return points.Sum() / points.Count;
        }
    }
}
