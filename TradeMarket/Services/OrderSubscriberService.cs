using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.OrderSubscriberService.v1;
using TradeMarket.Model;

namespace TradeMarket.Services
{
    public class OrderSubscriberService : TradeBot.Common.OrderSubscriberService.v1.OrderSubscriberService.OrderSubscriberServiceBase
    {
        private SubscriptionService<SubscribeOrdersRequest, SubscribeOrdersResponse, FullOrder, OrderSubscriberService> _subscriptionService;

        private ILogger<OrderSubscriberService> _logger;

        private static SubscribeOrdersResponse Convert(FullOrder order)
        {
            return new SubscribeOrdersResponse
            {
                SimpleOrderInfo = new TradeBot.Common.Order
                {
                    LastUpdateDate = new Timestamp()
                    {
                        Seconds = order.LastUpdateDate.Second
                    },
                    Price = order.Price,
                    Quantity = order.Quantity,
                    Signature = order.Signature,
                },
                RemoveDate = new Timestamp()
                {
                    Seconds = order.RemoveDate.Second
                },
                Id = order.Id,
                RemovePrice = order.RemovePrice
            };
        }


        public FormerService(FakeOrderSubscriber subscriber, ILogger<FormerService> logger)
        {
            _subscriptionService = new(subscriber, logger, Convert);
            _logger = logger;

        }


        /*public override Task<BuyOrderReply> BuyOrder(BuyOrderRequest request, ServerCallContext context)
        {
            return Task.FromResult(new BuyOrderReply
            {
                Reply = new TradeBot.Common.DefaultReply
                {
                    Code = TradeBot.Common.ReplyCode.Succeed,
                    Message = "Order placed"
                }
            }) ;
        }*/

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override async Task SubscribeOrders(SubscribeOrdersRequest request, IServerStreamWriter<SubscribeOrdersReply> responseStream, ServerCallContext context)
        {
            await _subscriptionService.Subscribe(request, responseStream, context);
        }
    }
    public override Task SubscribeOrders(SubscribeOrdersRequest request, IServerStreamWriter<SubscribeOrdersResponse> responseStream, ServerCallContext context)
        {
            return base.SubscribeOrders(request, responseStream, context);
        }
    }
}
