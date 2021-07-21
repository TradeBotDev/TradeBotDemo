using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.Logging;

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using TradeBot.TradeMarket.TradeMarketService.v1;

using TradeMarket.Model;

namespace TradeMarket.Services
{
    public class TradeMarketService : TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceBase
    {
        /* private SubscriptionService<SubscribeOrdersRequest, SubscribeOrdersResponse, FullOrder, TradeMarketService> _orderSubscriptionService;

         private SubscriptionService<SubscribeBalanceRequest, SubscribeBalanceResponse, Balance, TradeMarketService> _balanceSubscriptionService;

         private SubscriptionService<SlotsRequest, SlotsResponse, Slot, TradeMarketService> _slotSubscriptionService;*/


        private readonly ILogger<TradeMarketService> _logger;

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
            return new()
            {
                Response = new TradeBot.Common.v1.SubscribeBalanceResponse
                {
                    Balance = new TradeBot.Common.v1.Balance
                    {
                        Currency = balance.Currency,
                        Value = balance.Value.ToString(CultureInfo.InvariantCulture)
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
            /*//TODO Денис Тут надо тянуть зависимость на subscriber а не хардкодить
            //_orderSubscriptionService = new(BitmexPublisher.GetInstance(), logger, ConvertOrder);
            _balanceSubscriptionService = new(FakeBalanceSubscriber.GetInstance(), logger, ConvertBalance);
            _slotSubscriptionService = new(FakeSlotSubscriber.GetInstance(), logger, ConvertSlot);*/
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

        public override async Task<CloseOrderResponse> CloseOrder(CloseOrderRequest request, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionId").Value;
            try
            {
                var user = DataTransfering.TradeMarket.GetUserContextBySessionId(sessionId);
                await user.CloseOrder(request.Id);
            }
            catch (Exception)
            {
            }

            return (new CloseOrderResponse
            {
                Response = new TradeBot.Common.v1.DefaultResponse
                {
                    Code = TradeBot.Common.v1.ReplyCode.Succeed,
                    Message = $"Order {request.Id} was closed"
                }

            });
        }



        public override async Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
        {

            var sessionId = context.RequestHeaders.Get("sessionId").Value;


            var user = DataTransfering.TradeMarket.GetUserContextBySessionId(sessionId);
            var response = await user.PlaceOrder(request.Value, request.Price);

            return new PlaceOrderResponse
            {
                Response = response
            };
        }

        public override async Task Slots(SlotsRequest request, IServerStreamWriter<SlotsResponse> responseStream, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionId").Value;


            var user = DataTransfering.TradeMarket.GetUserContextBySessionId(sessionId);

            SubscriptionService<SlotsRequest, SlotsResponse, Slot, TradeMarketService> service = null; //= new (FakeSlotSubscriber.GetInstance().Changed,_logger,ConvertSlot);

            await service.Subscribe(request, responseStream, context);
        }

        private async Task WriteStreamAsync<TResponse>(IServerStreamWriter<TResponse> stream, TResponse reply) where TResponse : IMessage<TResponse>
        {
            try
            {
                await stream.WriteAsync(reply);
            }
            catch (Exception exception)
            {
                //TODO что делать когда разорвется соеденение ?
                _logger.LogWarning("Connection was interrupted by network services.");
            }
        }

        public override async Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionId").Value;
            var user = DataTransfering.TradeMarket.GetUserContextBySessionId(sessionId);
            user.UserBalance += async (_, args) =>
            {
                await WriteStreamAsync(responseStream, ConvertBalance(args));
            };

            var service = new SubscriptionService<SubscribeBalanceRequest, SubscribeBalanceResponse, Balance, TradeMarketService>(
                    user.InvokeDelegate, _logger, ConvertBalance);


            //TODO отписка после отмены
            await AwaitCancellation(context.CancellationToken);
        }

        public override Task SubscribeLogs(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            return base.SubscribeLogs(request, responseStream, context);
        }

        public override async Task SubscribeOrders(SubscribeOrdersRequest request, IServerStreamWriter<SubscribeOrdersResponse> responseStream, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionId").Value;
            var user = DataTransfering.TradeMarket.GetUserContextBySessionId(sessionId);
            user.Book25 += async (sender, args) =>
            {
                await WriteStreamAsync<SubscribeOrdersResponse>(responseStream, ConvertOrder(args));
            };
            //TODO отписка после отмены
            await AwaitCancellation(context.CancellationToken);
        }

        private static Task AwaitCancellation(CancellationToken token)
        {
            var completion = new TaskCompletionSource<object>();
            token.Register(() => completion.SetResult(null));
            return completion.Task;
        }
    }
}
