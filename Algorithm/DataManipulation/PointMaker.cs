using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Algorithm.DataManipulation
{
    public class PointMaker : IPointRepository
    {
        public Dictionary<DateTime, double> GetPoints(string SlotID)
        {
            Dictionary<DateTime, double> requestedPoints = new Dictionary<DateTime, double>();
            return requestedPoints;
        }


    }
}
