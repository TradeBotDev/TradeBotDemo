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
using TradeMarket.DataTransfering.Bitmex.Model;
using TradeMarket.Model.Publishers;
using TradeMarket.Model.UserContexts;
using Xunit;

namespace TradeMarketTests.IntegrationTests
{
    public class UnsubsribeTest
    {
        [Fact]
        public async Task Canceled_ShouldNotRecievedNewMessagesFromTradeMarketAfterCancelAsync()
        {
            //arrange
            var publisherFactory = new BitmexPublisherFactory(null);
            TradeMarket.Model.TradeMarkets.TradeMarket tm;
            var communicator = new BitmexWebsocketCommunicator(BitmexValues.ApiWebsocketTestnetUrl);
            var client = new BitmexWebsocketClient(communicator);

            var builder = new BitmexTradeMarketBuilder();
            tm = builder
                .AddConnectionMultiplexer(null)
                .AddCommonClient(client)
                .AddPublisherFactory(publisherFactory)
                .Result;
            var context1 = new CommonContext(tm, "XBTUSD");

            int count = 0;
            CancellationTokenSource source1 = new CancellationTokenSource();

            EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler1 = (sender, args) => { };
            Action<Websocket.Client.ResponseMessage> action = (message) => { count++; };
            //setup
            await communicator.Start();
            //act

            var reciever = communicator.MessageReceived.Subscribe(action);
            

                await context1.SubscribeToBook25UpdatesAsync(handler1, source1.Token);
                await Task.Delay(10_000);
                source1.Cancel();
            
            int stoppedCount = count;
            await context1.UnSubscribeFromBook25UpdatesAsync(handler1);
            await Task.Delay(50_000);


            //assert
            //10 это магическое число которое нужно т.к. на сервер должна успеть прилететь сообщение об отмене подписки
            Assert.True( count - stoppedCount < 10);
        }
    }
}
