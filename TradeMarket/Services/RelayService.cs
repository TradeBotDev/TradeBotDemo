using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.DataTransfering;
using TradeMarket.Model;
using TradeMarket.Relay;


namespace TradeMarket.Services
{
    public class RelayService: OrderProviderService.OrderProviderServiceBase
    {
        private SubscriptionService<SubscribeOrdersRequest, SubscribeOrdersReply,FullOrder, RelayService> _subscriptionService;

        private ILogger<RelayService> _logger;

        private SubscribeOrdersReply Convert(FullOrder order)
        {
            return new SubscribeOrdersReply
            {
                SimpleOrderInfo = new TradeBot.Common.Order
                {
                    Price = order.Price,
                    Quantity = order.Quantity,
                    Signature = order.Signature,
                    LastUpdateDate = new Timestamp()
                    {
                        Seconds = order.LastUpdateDate.Second
                    }
                }
            };
        }

        public RelayService(FakeOrderSubscriber subscriber, ILogger<RelayService> logger)
        {
            _subscriptionService = new(subscriber,logger,Convert);
            _logger = logger;
        }

        public async override Task SubscribeOrders(SubscribeOrdersRequest request, IServerStreamWriter<SubscribeOrdersReply> responseStream, ServerCallContext context)
        {
            await _subscriptionService.Subscribe(request, responseStream, context);
        }
    }
}
