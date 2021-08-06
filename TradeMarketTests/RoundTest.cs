using System;
using TradeMarket.DataTransfering.Bitmex;
using Xunit;

namespace TradeMarketTests
{
    public class RoundTest
    {
        [Fact]
        public void Test1()
        {
            var rounded = BitmexTradeMarket.RoundToHalfOrZero(27123.5);
            Assert.Equal(7, rounded.ToString().Length);
            Assert.Equal(27123.5, rounded);
        }

        [Fact]
        public void Test2()
        {
            var rounded = BitmexTradeMarket.RoundToHalfOrZero(27123.62);
            Assert.Equal(5, rounded.ToString().Length);
            Assert.Equal(27123, rounded);
        }


        [Fact]
        public void Test3()
        {
            var rounded = BitmexTradeMarket.RoundToHalfOrZero(27123.4);
            Assert.Equal(5, rounded.ToString().Length);
            Assert.Equal(27123, rounded);
        }

        [Fact]
        public void Test4()
        {
            var rounded = BitmexTradeMarket.RoundToHalfOrZero(27123.000000000001);
            Assert.Equal(5, rounded.ToString().Length);
            Assert.Equal(27123, rounded);
        }

        [Fact]
        public void Test5()
        {
            var rounded = BitmexTradeMarket.RoundToHalfOrZero(27123.5001234560001);

            Assert.Equal(7, rounded.ToString().Length);
            Assert.Equal(27123.5, rounded);
        }

        [Fact]
        public void Test6()
        {
            var rounded = BitmexTradeMarket.RoundToHalfOrZero(27123.4991234560001);

            Assert.Equal(7, rounded.ToString().Length);
            Assert.Equal(27123.5, rounded);
        }
    }
}
