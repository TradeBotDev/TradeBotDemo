using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.DataTransfering;
using TradeMarket.Former;
using TradeMarket.Model;

namespace TradeMarket.Services
{
    public class FormerService : TradeMarket.Former.OrderFormerService.OrderFormerServiceBase
    {
        private SubscriptionService<SubscribeOrdersRequest, SubscribeOrdersReply,FullOrder, FormerService> _subscriptionService;

        private ILogger<FormerService> _logger;

        private static TradeMarket.Former.SubscribeOrdersReply Convert(FullOrder order)
        {
            return new TradeMarket.Former.SubscribeOrdersReply
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
    

    private FormerService(FakeOrderSubscriber subscriber,ILogger<FormerService> logger)
        {
            _subscriptionService = new(subscriber, logger, Convert);
            _logger = logger;
            
        }

        public override Task<BuyOrderReply> BuyOrder(BuyOrderRequest request, ServerCallContext context)
        {
            return Task.FromResult(new BuyOrderReply
            {
                Reply = new TradeBot.Common.DefaultReply
                {
                    Code = TradeBot.Common.ReplyCode.Succeed,
                    Message = "Order placed"
                }
            }) ;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override async Task SubscribeOrders(SubscribeOrdersRequest request, IServerStreamWriter<SubscribeOrdersReply> responseStream, ServerCallContext context)
        {
            await _subscriptionService.Subscribe(request, responseStream, context);
            
        }
    }
}
