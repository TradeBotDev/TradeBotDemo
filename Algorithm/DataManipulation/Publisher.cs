using System;
using System.Collections.Generic;

namespace Algorithm.DataManipulation
{
    public class Publisher
    {
        public delegate void PointMade(KeyValuePair<DateTime, double> point);
        public PointMade PointMadeEvent;

        public void Publish(KeyValuePair<DateTime, double> point)
        {
            PointMadeEvent?.Invoke(point);
        }
    }
}
