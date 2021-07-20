using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Algorithm.Analysis
{
    //alpha takes all the points, calculates average with and without the last point
    //if the trend had been descending for three poins and then started rising we're buying 
    public class AlgorithmAlpha : IAlgorithm
    {
        public double CalculateSuggestedPrice()
        {
            throw new NotImplementedException();
        }

        public double CalculateSuggestedPrice(Dictionary<DateTime, double> points)
        {

        }
    }
}
