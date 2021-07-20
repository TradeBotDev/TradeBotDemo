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
        private Dictionary<DateTime, double> storage = new();
        public double CalculateSMA(Dictionary<DateTime, double> points)
        {
            double sum = 0;
            foreach(KeyValuePair<DateTime, double> point in points)
            {
                sum += point.Value;
            }

            return sum/points.Count;

        }
        //hardcoded for five points!! 
        //
        public bool IsItTimeToBuy(Dictionary<DateTime, double> points)
        {

            return false;
        }

        public double CalculateSuggestedPrice(Dictionary<DateTime, double> points)
        {
            return 0;
        }
    }
}
