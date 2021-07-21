using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Algorithm.DataManipulation
{
    public class Publisher
    {
        public event EventHandler<KeyValuePair<DateTime, double>> Handler = delegate { };

        public void Publish(KeyValuePair<DateTime, double> point)
        {
            Handler?.Invoke(this, point);
        }
    }


}
