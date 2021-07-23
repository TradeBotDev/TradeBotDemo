using System;
using System.Threading;


namespace Algorithm.Analysis
{
    public class AlgorithmEmulator
    {
        public double CalculateSuggestedPrice()
        {
            var rn = new Random();
            Thread.Sleep(rn.Next(0, 10000));
            return rn.Next(0, 20);
        }
    } 
}
