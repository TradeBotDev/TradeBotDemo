using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common;

namespace Algorithm
{
    public class AlgorithmEmulator : IAlgorithm
    {
        public double CalculateSuggestedPrice(IEnumerable<Order> orders, AlgorithmInfo algorithmInfo)
        {
            return 10;
        }

    }
}
