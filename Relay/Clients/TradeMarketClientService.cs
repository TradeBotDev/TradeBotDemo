using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;
using SubscribeOrdersRequest = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersRequest;
using SubscribeOrdersResponse = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersResponse;

namespace Relay.Clients
{
    public class TradeMarketClientService
    {
        private IAsyncStreamReader<SubscribeOrdersResponse> _stream;

        public TradeMarketClientService(Uri uri)
        {
            var client = new TradeMarketService.TradeMarketServiceClient(GrpcChannel.ForAddress(uri));
            _stream = client.SubscribeOrders(new SubscribeOrdersRequest()
            {
                Request = new TradeBot.Common.v1.SubscribeOrdersRequest()
                {
                    Signature = new OrderSignature()
                    {
                        Type = OrderType.Unspecified,
                        Status = OrderStatus.Closed
                    }
                }
            }).ResponseStream;
        }

        public delegate void OrderRecieved(object sender, OrderRecievedEventArgs args);

        public class OrderRecievedEventArgs
        {
            public Order Recieved { get; private set; }

            public OrderRecievedEventArgs(Order recieved)
            {
                Recieved = recieved;
            }
        }

        public static event OrderRecieved OrderRecievedEvent;

        public async Task ReadOrders()
        {
            while (await _stream.MoveNext())
            {
                OrderRecievedEvent?.Invoke(this,new (_stream.Current.Response.Order));
            }
        }
    }
}
