using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Communicator;
using Bitmex.Client.Websocket.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Bitmex.Client.Websocket.Messages;
using Bitmex.Client.Websocket.Requests;

namespace TradeMarketTests.EventsTests
{
    public class WebsocketPublishersTest
    {
        [Fact]
        public void test1()
        {
            IBitmexCommunicator commutator = new BitmexWebsocketCommunicator(new Uri("http://localhost/2134"));
            Moq.Mock<BitmexWebsocketClient> clientmoq = new Mock<BitmexWebsocketClient>(MockBehavior.Strict);
            Moq.Mock<BitmexWebsocketCommunicator> commmoq = new Mock<BitmexWebsocketCommunicator>(MockBehavior.Strict);
            //clientmoq.Setup(mq => mq.Send(It.IsAny<RequestBase>())).Callback(() => )
            BitmexWebsocketClient client = new BitmexWebsocketClient(commmoq.Object);

            //commmoq.Setup(cmq => cmq.Send(It.IsAny<string>())).Callback(() => client.Streams)


        }
    }
}
