using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;

namespace FormerTests
{
    public class FormerTest
    {
        [Fact]
        public void SomeAssertions() 
        {
            var lol = new Mock<Former.TradeMarketClient>();
        }

    }
}
