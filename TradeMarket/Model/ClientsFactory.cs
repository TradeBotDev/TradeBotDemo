using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;

namespace TradeMarket.Model
{
    public static class ClientsFactory
    {
        public static BitmexWebsocketClient CreateWebsocketClient(Uri connectionString)
        {
            var communicator = new BitmexWebsocketCommunicator(connectionString);
            var res = new BitmexWebsocketClient(communicator);
            communicator.ReconnectTimeout = null;
            communicator.Start();
            return res;
        }

        public static RestfulClient CreateRestfulClient(Uri conntectionString)
        {
            return new RestfulClient(conntectionString);
        }
    }
}
