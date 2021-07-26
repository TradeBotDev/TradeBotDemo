using System;
using System.Threading;


namespace Algorithm.Analysis
{
    public class AlgorithmEmulator
    {
        //left this in case we need a placeholder 
        public double CalculateSuggestedPrice()
        {
            var rn = new Random();
            Thread.Sleep(rn.Next(0, 10000));
            return rn.Next(0, 33000);
        }
    } 
}
