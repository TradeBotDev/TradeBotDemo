using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Margins;
using Bitmex.Client.Websocket.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Model;
using TradeMarket.Model.Publishers;
using TradeMarket.Model.UserContexts;
using TradeMarket.Model.UserContexts.Builders;
using Xunit;

namespace TradeMarketTests.IntegrationTests
{
    public class CacheTest
    {
        [Fact]
        public async Task Cache_ShouldNotContainsEmptyValuesAsync()
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
            var context = new BitmexContext();
            var iContextBuilder = new ContextBuilder();
            iContextBuilder.AddUniqueInformation("XBTUSD", "123");
            var contextBuilder = new BitmexContextBuilder(iContextBuilder);
            contextBuilder
                .AddTradeMarket(tm)
                .AddKeySecret(key: "0n8sicC9Y8v3iuwtDDkJ44IO", secret: "PhVLNBRGA199lGgrQ2bbf59Ux7yRsgwkn-sfigW7rMOPoPWh")
                .AddWebSocketClient(client);
            context =await contextBuilder.InitUser(new System.Threading.CancellationToken());
            


            CancellationTokenSource source1 = new CancellationTokenSource();
            CancellationTokenSource source2 = new CancellationTokenSource();

            int count = 0;
            EventHandler<IPublisher<Margin>.ChangedEventArgs> handler = (sender, args) => { count++; };

            //act
            await context.SubscribeToUserMargin(handler, source1.Token);
            await Task.Delay(3_000);
            source1.Cancel();
            var cache = await context.SubscribeToUserMargin(handler, source2.Token);
            source2.Cancel();


            //assert
            foreach (var data in cache)
            {
                //данные которые передаются по рцпшкам
                if (data.AvailableMargin is null || data.AvailableMargin == 0 ||
                   data.RealisedPnl is null || data.RealisedPnl == 0 ||
                   data.MarginBalance is null || data.MarginBalance == 0
                    )
                {
                    //assert fail по сишарповски
                    Assert.False(true);
                }
            }
            Assert.True(true);
        }
    }
}
