using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.TradeMarket.TradeMarketService.v1;
using TradeMarket.DataTransfering;
using TradeMarket.Model;

namespace TradeMarket.Services
{
    public class TradeMarketService : TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceBase
    {
        private SubscriptionService<SubscribeOrdersRequest, SubscribeOrdersResponse, FullOrder, TradeMarketService> _orderSubscriptionService;

        private SubscriptionService<SubscribeBalanceRequest, SubscribeBalanceResponse, Balance, TradeMarketService> _balanceSubscriptionService;

        private SubscriptionService<SlotsRequest, SlotsResponse, Slot, TradeMarketService> _slotSubscriptionService;


        private ILogger<TradeMarketService> _logger;

        private static SubscribeOrdersResponse ConvertOrder(FullOrder order)
        {
            return new SubscribeOrdersResponse
            {
                Response = new TradeBot.Common.v1.SubscribeOrdersResponse
                {
                    Order = new TradeBot.Common.v1.Order
                    {
                        Id = order.Id,
                        LastUpdateDate = new Timestamp
                        {
                            Seconds = order.LastUpdateDate.Second
                        },
                        Price = order.Price,
                        Quantity = order.Quantity,
                        Signature = order.Signature
                    }
                }
            };
        }

        private static SubscribeBalanceResponse ConvertBalance(Balance balance)
        {
            return new SubscribeBalanceResponse
            {
                Response = new TradeBot.Common.v1.SubscribeBalanceResponse
                {
                    Balance = new TradeBot.Common.v1.Balance
                    {
                        Currency = balance.Currency,
                        Value = balance.Value.ToString()
                    }
                }
            };
        }

        private static SlotsResponse ConvertSlot(Slot slot)
        {
            return new SlotsResponse
            {
                SlotName = slot.Name
            };
        }


        public TradeMarketService(ILogger<TradeMarketService> logger)
        {
            //TODO Денис Тут надо тянуть зависимость на subscriber а не хардкодить
            _orderSubscriptionService = new(BitmexOrderSubscriber.GetInstance(), logger, ConvertOrder);
            _balanceSubscriptionService = new(FakeBalanceSubscriber.GetInstance(), logger, ConvertBalance);
            _slotSubscriptionService = new(FakeSlotSubscriber.GetInstance(), logger, ConvertSlot);
            _logger = logger;
        }

        public override Task<AuthenticateTokenResponse> AuthenticateToken(AuthenticateTokenRequest request, ServerCallContext context)
        {
            return Task.FromResult(new AuthenticateTokenResponse
            {
                Response = new TradeBot.Common.v1.DefaultResponse
                {
                    Code = TradeBot.Common.v1.ReplyCode.Succeed,
                    Message = $"Token {request.Token} was authtarized"
                }

            });
        }

        public override Task<CloseOrderResponse> CloseOrder(CloseOrderRequest request, ServerCallContext context)
        {
            return Task.FromResult(new CloseOrderResponse
            {
                Response = new TradeBot.Common.v1.DefaultResponse
                {
                    Code = TradeBot.Common.v1.ReplyCode.Succeed,
                    Message = $"Order {request.Id} was closed"
                }

            });
        }

        public override Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PlaceOrderResponse
            {
                Response = new TradeBot.Common.v1.DefaultResponse
                {
                    Code = TradeBot.Common.v1.ReplyCode.Succeed,
                    Message = $"Order with price : {request.Price} and value: {request.Value} was placed with Id : {Guid.NewGuid().ToString()}"
                }

            }); 
        }

        public async override Task Slots(SlotsRequest request, IServerStreamWriter<SlotsResponse> responseStream, ServerCallContext context)
        {
            await _slotSubscriptionService.Subscribe(request, responseStream, context);
        }

        public async  override Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {
            
            await _balanceSubscriptionService.Subscribe(request, responseStream, context);
        }

        public override Task SubscribeLogs(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            return base.SubscribeLogs(request, responseStream, context);
        }

        public async override Task SubscribeOrders(SubscribeOrdersRequest request, IServerStreamWriter<SubscribeOrdersResponse> responseStream, ServerCallContext context)
        {
            await _orderSubscriptionService.Subscribe(request, responseStream, context);
        }
    }
}
