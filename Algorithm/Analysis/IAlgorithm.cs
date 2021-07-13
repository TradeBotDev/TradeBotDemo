using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common;

namespace Algorithm
{
    interface IAlgorithm
    {
        public double CalculateSuggestedPrice(IEnumerable<Order> orders, AlgorithmInfo algorithmInfo);
    }
}
