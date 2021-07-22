using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Algorithm.DataManipulation
{
    public class Publisher
    {
        public delegate void PointMade(KeyValuePair<DateTime, double> point);
        public PointMade pointMadeEvent;

        public void Publish(KeyValuePair<DateTime, double> point)
        {
            pointMadeEvent?.Invoke(point);
        }
    }


}
