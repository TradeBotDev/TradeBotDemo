using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common;
using System.Threading;
using Grpc.Core;


namespace Algorithm
{
    public class AlgorithmEmulator : IAlgorithm
    {
        public double CalculateSuggestedPrice()
        {
            Random rn = new Random();
            Thread.Sleep(rn.Next(0, 10000));
            return rn.Next(31400, 31600);
        }

    }
  
}
