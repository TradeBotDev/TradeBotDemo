using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Websockets;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Clients;
using TradeMarket.DataTransfering.Bitmex.Model;
using TradeMarket.Model;
using TradeMarket.Model.Publishers;
using TradeMarket.Model.TradeMarkets;
using TradeMarket.Model.UserContexts;
using TradeMarket.Model.UserContexts.Builders;
using TypeMock.ArrangeActAssert;
using Xunit;

namespace TradeMarketTests.IntegrationTests
{
    public class UserContextTest
    {
        [Fact]
        public async void MultipleUsers_ShouldCreatesOneInstanceOfUserContext()
        {
            /*//arrange 
            string sessionId = "123";
            string slot = "XBTUSD";
            string trademarket = "bitmex";

            Mock<AccountClient> account = new Mock<AccountClient>(MockBehavior.Strict, new object[] { null });
            account.SetReturnsDefault<Task<UserAccessInfo>>(Task.FromResult(new UserAccessInfo("key", "secret")));

            Mock<TradeMarket.Model.TradeMarkets.TradeMarket> tm = new Mock<TradeMarket.Model.TradeMarkets.TradeMarket>(MockBehavior.Strict);
            tm.SetupGet(mq => mq.Name).Returns("bitmex");

            Mock<TradeMarketFactory> factory = new Mock<TradeMarketFactory>(MockBehavior.Strict);
            factory.Setup(mq => mq.GetTradeMarket(It.IsAny<string>())).Returns(tm.Object);
            var director = new ContextDirector(new UserContextBuilder(),account.Object, factory.Object);

            //action
            foreach(int val in Enumerable.Range(0, 10))
            {
                _ = await director.GetUserContextAsync(sessionId, slot, trademarket);
            }

            //assert
            Assert.Equal(1, director.RegisteredUsersCount);*/
        }
        
        [Fact]
        public async void MultipleUser_ShouldCreatesOneInstanceOfUserContextAsync()
        {
            /*//arrange 
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
            Assert.Equal(1, director.RegisteredUsersCount);*/
        }

        [Fact]
        public async Task CancelationRequested_ShouldNotKillConnectionForOtherSubscribres()
        {
            //arrange
            var publisherFactory = new BitmexPublisherFactory(null);
            TradeMarket.Model.TradeMarkets.TradeMarket tm;
            var communicator = new BitmexWebsocketCommunicator(BitmexValues.ApiWebsocketTestnetUrl);
            var client = new BitmexWebsocketClient(communicator);
            await communicator.Start();
            var builder = new BitmexTradeMarketBuilder();
            tm = builder
                .AddConnectionMultiplexer(null)
                .AddCommonClient(client)
                .AddPublisherFactory(publisherFactory)
                .Result;
            var context1 = new CommonContext(tm, "XBTUSD");
            var context2 = new CommonContext(tm, "XBTUSD");

            CancellationTokenSource source1 = new CancellationTokenSource();
            CancellationTokenSource source2 = new CancellationTokenSource();

            int count1 = 0;
            int count2 = 0;
            EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler1 = (sender, args) => {
                count1++; 
            };
            EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler2 = (sender, args) => { count2++; };

            //act
            await context1.SubscribeToBook25UpdatesAsync(handler1, source1.Token);
            await context2.SubscribeToBook25UpdatesAsync(handler2, source2.Token);
            await Task.Delay(1000);
            source1.Cancel();
            await context1.UnSubscribeFromBook25UpdatesAsync(handler1);
            await Task.Delay(5000);
            source2.Cancel();
            await context2.UnSubscribeFromBook25UpdatesAsync(handler2);


            //assert
            Assert.True(count2 > count1);
        }
    }
}
