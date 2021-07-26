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
    public class TradeMarketClient 
    {
        private TradeMarketService.TradeMarketServiceClient _client;

        public TradeMarketClient(Uri uri)
        {
             _client = new TradeMarketService.TradeMarketServiceClient(GrpcChannel.ForAddress(uri));   
        }

        public IAsyncStreamReader<SubscribeOrdersResponse> OpenStream(Metadata meta)
        {
            return _client.SubscribeOrders(new SubscribeOrdersRequest()
            {
                Request = new TradeBot.Common.v1.SubscribeOrdersRequest()
                {
                    Signature = new OrderSignature()
                    {
                        Type = OrderType.Unspecified,
                        Status = OrderStatus.Closed
                    }
                }
            }, meta).ResponseStream;
        }

        public async void SubscribeForOrders(IAsyncStreamReader<SubscribeOrdersResponse> stream)
        {
            while (await stream.MoveNext())
            {
                OrderRecievedEvent?.Invoke(this, new(stream.Current.Response.Order));
            }
        }

        public event EventHandler<Order> OrderRecievedEvent;
    }
}
