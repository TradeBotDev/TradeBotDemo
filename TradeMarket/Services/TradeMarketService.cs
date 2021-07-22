using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeBot.TradeMarket.TradeMarketService.v1;
using TradeMarket.DataTransfering;
using TradeMarket.Model;
using TradeMarket.Services;
using SubscribeBalanceRequest = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceRequest;
using SubscribeBalanceResponse = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceResponse;
using SubscribeOrdersResponse = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersResponse;

namespace TradeMarket.Services
{
    public class TradeMarketService : TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceBase
    {
       private ILogger<TradeMarketService> _logger;

        private static SubscribeOrdersResponse ConvertOrder(FullOrder order)
        {
            return new ()
            {
                Response = new ()
                {
                    Order = new ()
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

        private static SubscribeBalanceResponse ConvertBalance(Model.Balance balance)
        {
            return new ()
            {
                Response = new ()
                {
                    Balance = new ()
                    {
                        Currency = balance.Currency,
                        Value = balance.Value.ToString()
                    }
                }
            };
        }

        private static SlotsResponse ConvertSlot(Slot slot)
        {
            return new ()
            {
                SlotName = slot.Name
            };
        }


        public TradeMarketService(ILogger<TradeMarketService> logger)
        {
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

        public async override Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
        {

            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var user = Model.TradeMarket.GetUserContex(sessionId, slot);

            var response = await user.PlaceOrder(request.Value, request.Price);

            return new PlaceOrderResponse
            {
                Response = response
            };
        }

        public async override Task Slots(SlotsRequest request, IServerStreamWriter<SlotsResponse> responseStream, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;

            var user = Model.TradeMarket.GetUserContex(sessionId, slot);

            FakeSlotPublisher.GetInstance().Changed += async (sender, args) =>
            {
                await WriteStreamAsync<SlotsResponse>(responseStream, ConvertSlot(args.Changed));
            };
            //TODO отписка после отмены
            await AwaitCancellation(context.CancellationToken);

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

        public async override Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var user = Model.TradeMarket.GetUserContex(sessionId,slot);

            user.UserBalance += async (sender, args) => {
                await WriteStreamAsync<SubscribeBalanceResponse>(responseStream, ConvertBalance(args));
            };
            //TODO отписка после отмены
            await AwaitCancellation(context.CancellationToken);

        }

        public override Task SubscribeLogs(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            return base.SubscribeLogs(request, responseStream, context);
        }

        public async override Task SubscribeOrders(SubscribeOrdersRequest request, IServerStreamWriter<SubscribeOrdersResponse> responseStream, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var user = Model.TradeMarket.GetUserContex(sessionId, slot);
            user.Book25 += async (sender, args) => {
                var order = ConvertOrder(args);
                _logger.LogInformation($"Sent order : {order} to former");
                await WriteStreamAsync<SubscribeOrdersResponse>(responseStream, order);
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
