using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeMarket.Clients;
using TradeMarket.Model;
using TradeMarket.Model.TradeMarkets;
using TradeMarket.Model.UserContexts;
using TypeMock.ArrangeActAssert;
using Xunit;

namespace TradeMarketTests.IntegrationTests
{
    public class UserContextTest
    {
        [Fact]
        public async void MultipleUsers_ShouldCreatesOneInstanceOfUserContext()
        {
            //arrange 
            string sessionId = "123";
            string slot = "XBTUSD";
            string trademarket = "bitmex";

            Mock<AccountClient> account = new Mock<AccountClient>(MockBehavior.Strict, new object[] { null });
            account.SetReturnsDefault<Task<UserAccessInfo>>(Task.FromResult(new UserAccessInfo("key", "secret")));

            Mock<TradeMarket.Model.TradeMarkets.TradeMarket> tm = new Mock<TradeMarket.Model.TradeMarkets.TradeMarket>(MockBehavior.Strict);
            tm.SetupGet(mq => mq.Name).Returns("bitmex");

            Mock<TradeMarketFactory> factory = new Mock<TradeMarketFactory>(MockBehavior.Strict);
            factory.Setup(mq => mq.GetTradeMarket(It.IsAny<string>())).Returns(tm.Object);
            var director = new UserContextDirector(new UserContextBuilder(),account.Object, factory.Object);

            //action
            foreach(int val in Enumerable.Range(0, 10))
            {
                _ = await director.GetUserContextAsync(sessionId, slot, trademarket);
            }

            //assert
            Assert.Equal(1, director.RegisteredUsersCount);
        }
        
        [Fact]
        public async void MultipleUser_ShouldCreatesOneInstanceOfUserContextAsync()
        {
            //arrange 
            string sessionId = "123";
            string slot = "XBTUSD";
            string trademarket = "bitmex";

            AccountClient account = Isolate.Fake.Instance<AccountClient>();
            Isolate.WhenCalled(() => account.GetUserInfoAsync(sessionId)).WillReturn(Task.FromResult(new UserAccessInfo("key", "secret")));

            TradeMarket.Model.TradeMarkets.TradeMarket tm = Isolate.Fake.Instance<TradeMarket.Model.TradeMarkets.TradeMarket>();
            Isolate.WhenCalled(() => tm.Name).WillReturn("bitmex");
            Isolate.WhenCalled(() => tm.SubscribeToBalance()).
            tm.SetReturnsDefault<Task>(null);
            tm.SetupGet(mq => mq.Name).Returns("bitmex");


            Mock<TradeMarketFactory> factory = new Mock<TradeMarketFactory>(MockBehavior.Strict);
            factory.Setup(mq => mq.GetTradeMarket(It.IsAny<string>())).Returns(tm.Object);
            var director = new UserContextDirector(new UserContextBuilder(), account.Object, factory.Object);

            //act
            Parallel.ForEach(Enumerable.Range(0, 10), async el => await director.GetUserContextAsync(sessionId, slot, trademarket));

            //assert
            Assert.Equal(1, director.RegisteredUsersCount);
        }
    }
}
