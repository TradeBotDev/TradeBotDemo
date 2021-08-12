using System;
using System.Collections.Generic;

namespace Algorithm.DataManipulation
{
    //used to implement Pub-Sub model, used to notify algo and DataCollector that a point was made 
    public class PointPublisher
    {
        public delegate void PointMade(KeyValuePair<DateTime, double> point);
        public PointMade PointMadeEvent;

        public void Publish(KeyValuePair<DateTime, double> point)
        {
            PointMadeEvent?.Invoke(point);
        }
    }
}
