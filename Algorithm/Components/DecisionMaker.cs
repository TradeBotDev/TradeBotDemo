using Algorithm.Components.Publishers;
using Algorithm.DataManipulation;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Algorithm.Components
{
    public class DecisionMaker
    {
        //algo's inner storage of points made by PointMaker
        //incoming into Algorithm service are all the orders from TM, so in order to simplify this data for calculations we transform orders into "point"
        //these points are basically the average market prices at some specific points in time
        //we make a point every second if possible
        private SortedDictionary<DateTime, double> _storage;

        //hardcoded, not sure if this will need to change
        private int _durationInPoints = 5;

        //0 - minimal, 1 - low, 2 - medium, 3 - high, 4 - max
        private int _precision = 0;
        private DecisionPublisher _decisionPublisher;
        private PointPublisher _pointPublisher;
        public DecisionMaker(DecisionPublisher decisionPublisher, PointPublisher pointPublisher)
        {
            _storage = new();
            _decisionPublisher = decisionPublisher;
            _pointPublisher = pointPublisher;
            _pointPublisher.PointMadeEvent += NewPointAlert;
        }

        //when a new point is made algo adds it to its storage and checks if it has enough to initiate analysis 
        //if the storage is overfilled we remove the oldest point
        private void NewPointAlert(KeyValuePair<DateTime, double> point)
        {
            Log.Information("{@Where}: Received a point for slot {@Slot}", "Algorithm", _metadata.GetValue("slot"));
            _storage.Add(point.Key, point.Value);

            if (_storage.Count > _durationInPoints)
            {
                var toRemove = _storage.First();
                _storage.Remove(toRemove.Key);
            }
            if (_storage.Count == _durationInPoints)
            {
                PerformCalculations(_storage);
            }
        }
        private void PerformCalculations(SortedDictionary<DateTime, double> points)
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
            int trend = MakeADecision(subTrends, points);
            if (trend != 0)
            {
                _decisionPublisher.Publish(trend);
            }
        }
        //this func needs points and subset averages and decides if it's time to buy
        private int MakeADecision(IReadOnlyCollection<double> subTrends, IDictionary<DateTime, double> points)
        {
            Log.Information("{@Where}:Analysing the following points: " + string.Join(Environment.NewLine, points), "Algorithm");
            if (_precision == 0)
            {
                return AnalyseTrendWithMinimalPrecision(points);
            }

            bool downtrend = true;
            bool uptrend = true;

            //if our averages are getting lower with each iteration we conclude the market price is falling
            for (int i = 1; i < subTrends.Count; i++)
            {
                //if any of the subset averages are bigger than the previous we assume the trend is NOT downward
                if (subTrends.ElementAt(i - 1) < subTrends.ElementAt(i))
                {
                    downtrend = false;
                    break;
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

            int trend = 0;

            if (downtrend || uptrend)
            {
                if (downtrend)
                {
                    Log.Information("{@Where}:Downward trend detected", "Algorithm");
                }
                else
                {
                    Log.Information("{@Where}:Upward trend detected", "Algorithm");
                }
                switch (_precision)
                {
                    case 1:
                        trend = AnalyseTrendWithLowPrecision(subTrends, points, uptrend);
                        break;
                    case 2:
                        trend = AnalyseTrendWithMediumPrecision(subTrends, points, uptrend);
                        break;
                    case 3:
                        trend = AnalyseTrendWithHighPrecision(subTrends, points, uptrend);
                        break;
                    case 4:
                        trend = AnalyseTrendWithUltraPrecision(subTrends, points, uptrend);
                        break;
                    default:
                        trend = AnalyseTrendWithHighPrecision(subTrends, points, uptrend);
                        break;
                }
            }

            if (trend != 0)
            {
                return trend;
            }
            return 0;
        }

        private static int AnalyseTrendWithMinimalPrecision(IDictionary<DateTime, double> prices)
        {
            if (prices.ElementAt(prices.Count - 4).Value < prices.ElementAt(prices.Count - 3).Value
                && prices.ElementAt(prices.Count - 3).Value < prices.ElementAt(prices.Count - 2).Value
                && prices.ElementAt(prices.Count - 2).Value > prices.Last().Value)
            {
                Log.Information("{@Where}:Upward trend in minimal precision", "Algorithm");
                return 1;
            }
            if (prices.ElementAt(prices.Count - 4).Value > prices.ElementAt(prices.Count - 3).Value
                && prices.ElementAt(prices.Count - 3).Value > prices.ElementAt(prices.Count - 2).Value
                && prices.ElementAt(prices.Count - 2).Value < prices.Last().Value)
            {
                Log.Information("{@Where}:Downward trend in minimal precision", "Algorithm");
                return -1;
            }
            return 0;
        }
        private static int AnalyseTrendWithLowPrecision(IReadOnlyCollection<double> subTrends, IDictionary<DateTime, double> prices, bool currentTrend)
        {
            if (currentTrend)
            {
                if (subTrends.Last() > prices.Last().Value)
                {
                    return -1;
                }
            }
            else
            {
                if (subTrends.Last() < prices.Last().Value)
                {
                    return 1;
                }
            }
            return 0;
        }

        private static int AnalyseTrendWithMediumPrecision(IReadOnlyCollection<double> subTrends, IDictionary<DateTime, double> prices, bool currentTrend)
        {
            int result = AnalyseTrendWithLowPrecision(subTrends, prices, currentTrend);

            switch(result)
            {
                case 0: return 0;
                case 1: if (prices.Last().Value > prices.ElementAt(prices.Count - 2).Value)
                    {
                        return 1;
                    }
                    break;
                case -1: if (prices.Last().Value < prices.ElementAt(prices.Count - 2).Value)
                    {
                        return -1;
                    }
                    break;
            }
            return 0;
        }
        private static int AnalyseTrendWithHighPrecision(IReadOnlyCollection<double> subTrends, IDictionary<DateTime, double> prices, bool currentTrend)
        {
            int result = AnalyseTrendWithMediumPrecision(subTrends, prices, currentTrend);

            switch (result)
            {
                case 0: return 0;
                case 1: if (prices.ElementAt(prices.Count - 3).Value > prices.ElementAt(prices.Count - 2).Value)
                    {
                        return 1;
                    }
                    break;
                case -1: if (prices.ElementAt(prices.Count - 3).Value < prices.ElementAt(prices.Count - 2).Value)
                    {
                        return -1;
                    }
                    break;
            }
            return 0;           
        }

        private static int AnalyseTrendWithUltraPrecision(IReadOnlyCollection<double> subTrends, IDictionary<DateTime, double> prices, bool currentTrend)
        {
            int result = AnalyseTrendWithHighPrecision(subTrends, prices, currentTrend);

            switch (result)
            {
                case 0: return 0;
                case 1: if (prices.ElementAt(prices.Count - 4).Value > prices.ElementAt(prices.Count - 3).Value)
                    {
                        return 1;
                    }
                    break;
                case -1: if (prices.ElementAt(prices.Count - 4).Value < prices.ElementAt(prices.Count - 3).Value)
                    {
                        return -1;
                    }
                    break;
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
