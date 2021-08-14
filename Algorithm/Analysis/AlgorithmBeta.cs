using Algorithm.DataManipulation;
using Grpc.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using TradeBot.Common.v1;

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

        //hardcoded, not sure if this will need to change
        private int _durationInPoints = 5;

        //1 - low, 2 - medium, 3 - high
        private int _precision = 1;

        private PointPublisher _pointPublisher;
        private DataCollector _dc;
        private PointMaker _pm;
        private bool _isStopped = true;

        private Metadata _metadata;

        public bool GetState()
        {
            return _isStopped;
        }

        //when an algo is created it's immediately subscribed to new points 
        public AlgorithmBeta(Metadata metadata)
        {
            Log.Information("IM AN ALGO, IM BEING CREATED RIGHT NOW");
            _metadata = metadata;
            _pointPublisher = new();
            _pointPublisher.PointMadeEvent += NewPointAlert;
            _dc = new(_pointPublisher);
            _pm = new();
            _storage = new Dictionary<DateTime, double>();
            Log.Information("Created an algo AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        }

        //when a new point is made algo adds it to its storage and checks if it has enough to initiate analysis 
        //if the storage is overfilled we remove the oldest point
        private void NewPointAlert(KeyValuePair<DateTime, double> point)
        {
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

        public void NewOrderAlert(Order order)
        {
            _dc.AddNewOrder(order);
        }

        //this function gathers all the data and sends it for analysis
        //
        private void PerformCalculations(Dictionary<DateTime, double> points)
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
                PriceSender.SendDecision(trend, _metadata);
            }
        }
        //this func needs points and subset averages and decides if it's time to buy
        private int MakeADecision(IReadOnlyCollection<double> subTrends, Dictionary<DateTime, double> points)
        {
            Log.Information("Analysis...");

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
                    Log.Information("Downward trend detected");
                }
                else
                {
                    Log.Information("Upward trend detected");
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
                    default:
                        trend = AnalyseTrendWithLowPrecision(subTrends, points, uptrend);
                        break;
                }
            }

            if (trend != 0)
            {
                return trend;
            }
            return 0;
        }

        private static int AnalyseTrendWithMinimalPrecision(Dictionary<DateTime, double> prices)
        {
            int trend = 0;
            if (prices.ElementAt(prices.Count - 3).Value < prices.ElementAt(prices.Count - 2).Value
                && prices.ElementAt(prices.Count - 3).Value < prices.ElementAt(prices.Count - 2).Value
                && prices.ElementAt(prices.Count - 2).Value > prices.Last().Value)
            {
                trend = 1;
            }
            if (prices.ElementAt(prices.Count - 3).Value > prices.ElementAt(prices.Count - 2).Value
                && prices.ElementAt(prices.Count - 3).Value > prices.ElementAt(prices.Count - 2).Value
                && prices.ElementAt(prices.Count - 2).Value < prices.Last().Value)
            {
                trend = -1;
            }
            return trend;
        }
        private static int AnalyseTrendWithLowPrecision(IReadOnlyCollection<double> subTrends, Dictionary<DateTime, double> prices, bool currentTrend)
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

        private static int AnalyseTrendWithMediumPrecision(IReadOnlyCollection<double> subTrends, Dictionary<DateTime, double> prices, bool currentTrend)
        {
            if (currentTrend)
            {
                if (subTrends.Last() > prices.Last().Value &&
                    prices.Last().Value < prices.ElementAt(prices.Count - 1).Value)
                {
                    return -1;
                }
            }
            else
            {
                if (subTrends.Last() < prices.Last().Value &&
                    prices.Last().Value > prices.ElementAt(prices.Count - 1).Value)
                {
                    return 1;
                }
            }
            return 0;
        }
        private static int AnalyseTrendWithHighPrecision(IReadOnlyCollection<double> subTrends, Dictionary<DateTime, double> prices, bool currentTrend)
        {
            if (currentTrend)
            {
                if (subTrends.Last() > prices.Last().Value &&
                    prices.Last().Value < prices.ElementAt(prices.Count - 1).Value &&
                    prices.ElementAt(prices.Count - 2).Value < prices.ElementAt(prices.Count - 1).Value)
                {
                    return -1;
                }
            }
            else
            {
                if (subTrends.Last() < prices.Last().Value &&
                    prices.Last().Value > prices.ElementAt(prices.Count - 1).Value &&
                    prices.ElementAt(prices.Count - 2).Value > prices.ElementAt(prices.Count - 1).Value)
                {
                    return 1;
                }
            }
            return 0;
        }
        //func to find average price
        private static double CalculateSMA(List<double> points)
        {
            return points.Sum() / points.Count;
        }

        public void ChangeSetting(AlgorithmInfo settings)
        {
            _precision = settings.Sensivity;
            _pm.SetPointInterval((int)settings.Interval.Seconds * 1000 / 5);
            _dc.ClearAllData();
            Log.Information("Settings changed");
        }
        public void ChangeState()
        {
            if (_isStopped)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }
        private void Stop()
        {
            _isStopped = true;
            _pm.Stop();
        }

        private void Start()
        {
            _isStopped = false;
            _pm.Launch(_pointPublisher, _dc);
        }
    }
}
