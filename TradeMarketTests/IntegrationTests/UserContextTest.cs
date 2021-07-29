using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeMarket.Model;
using Xunit;

namespace TradeMarketTests.IntegrationTests
{
    public class UserContextTest
    {
        [Fact]
        public async void MultipleUserShouldCreatesOneInstanceOfUserContextSync()
        {
            /*Moq.Mock<UserContext> UserContextMoq = new Moq.Mock<UserContext>(Moq.MockBehavior.Loose);
            UserContextMoq.Setup(uq => uq.init()).Callback(() => { });
            UserContextMoq.Setup(uq => uq.IsEquevalentTo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).CallBase();
            */
            var user1 = await UserContext.GetUserContextAsync("123", "123", "bitmex");
            var user2 = await UserContext.GetUserContextAsync("123", "123", "bitmex");
            var user3 = await UserContext.GetUserContextAsync("123", "123", "bitmex");
        }

        [Fact]
        public async void MultipleUserShouldCreatesOneInstanceOfUserContextAsync()
        {

        }
    }
}
