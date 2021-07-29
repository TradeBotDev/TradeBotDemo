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
            Moq.Mock<UserContext> UserContextMoq = new Moq.Mock<UserContext>(Moq.MockBehavior.Strict);
            UserContextMoq.Setup(uq => uq.init()).Callback(() => { });
            UserContextMoq.Setup(uq => uq.IsEquevalentTo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).CallBase();
            UserContextMoq.Setup()
            var user1 = await UserContextMoq.GetUserContextAsync("123", "123", "123");
        }

        [Fact]
        public async void MultipleUserShouldCreatesOneInstanceOfUserContextAsync()
        {

        }
    }
}
