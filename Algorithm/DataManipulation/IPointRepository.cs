using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Algorithm.DataManipulation
{
    interface IPointRepository
    {
        public Dictionary<DateTime, double> GetPoints(string SlotID);
    }
}
