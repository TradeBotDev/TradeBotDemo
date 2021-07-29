using System;
using Xunit;
using System.Collections.Generic;
using System.Linq;

using TradeBot.Algorithm;
using Algorithm.Analysis;
using Algorithm.DataManipulation;


namespace AlgorithmTest
{
    public class MathTests
    {
        [Fact]
        public void CalculateEMA_GivenEmptyList_ExpectedEmptyList()
        {
            //nothing to arrange

            List<double> result = AlgorithmBeta.CalculateEMA(new List<double>());

            Assert.Equal(new List<double>(), result);
        }

        [Fact]
        public void CalculateEMA_GivenOneNumber_ExpectedSameNumber()
        {
            //nothing to arrange 

            List<double> result = AlgorithmBeta.CalculateEMA(new List<double>() { 10 });

            Assert.Equal(10, result.ElementAt(0));
        }

        [Fact]
        public void CalculateEMA_GivenTwoNumbers_ExpectedParticularNumbers()
        {
            //nothing to arrange

            List<double> result = AlgorithmBeta.CalculateEMA(new List<double>() { 9, 12 });

            Assert.Equal(new List<double>() { 9, 11 }, result);
        }

        [Fact]
        public void CalculateSMA_GivenEmptyDict_ExpectedException()
        {
            //nothing to arrange

            var exception = Record.Exception(() => AlgorithmBeta.CalculateSMA(new Dictionary<DateTime, double>()));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void CalculateSMA_GivenOneNumber_ExpectedSameNumber()
        {
            var rnd = new Random();
            double num = rnd.Next(0, 10);
            var dict = new Dictionary<DateTime, double>();
            dict.Add(DateTime.Now, num);

            var result = AlgorithmBeta.CalculateSMA(dict);

            Assert.Equal(num, result);
        }

        [Fact]
        public void CalculateSMA_GivenTwoNumbers_ExpectedParticularNumber()
        {
            Dictionary<DateTime, double> dict = new();
            dict.Add(DateTime.Now, 8);
            dict.Add(DateTime.Now, 10);

            var result = AlgorithmBeta.CalculateSMA(dict);

            Assert.Equal(9, result);
        }
    }
}
