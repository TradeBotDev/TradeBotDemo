using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Former;
using Xunit;
using FormerService = Former.Former;

namespace FormerTests
{
    public class FitPriceTest
    {
        [Fact]
        public void Price_Range_Accurate_Enough()
        {
            var actiual = FormerService.RoundPriceRange(37654.5, 37642);
            Assert.Equal(12.5, actiual, 1);
        }

        [Fact]
        public void Could_Form_Price_From_Range_1()
        {
            var range = FormerService.RoundPriceRange(37654.5, 37653.5);
            Assert.Equal(1, range);
            var price = FormerService.MakePrice(range, 37654.5, TradeBot.Common.v1.OrderType.Sell);
            Assert.Equal(37654, price);
        }

        [Fact]
        public void Could_Not_Form_Price_From_Range_Less_Then_1()
        {
            double fairSellPrice = 37654.5;
            var range = FormerService.RoundPriceRange(fairSellPrice, 37654);
            Assert.Equal(0.5, range);
            var price = FormerService.MakePrice(range, 37654.5, TradeBot.Common.v1.OrderType.Sell);
            Assert.Equal(fairSellPrice, price);
        }
    }
}
