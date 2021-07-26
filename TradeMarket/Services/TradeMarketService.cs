﻿using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Serilog;
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

        private static SubscribeOrdersResponse ConvertOrder(FullOrder order)
        {
            return new ()
            {
                Response = new ()
                {
                    Order = Convert(order)
                }
            };
        }

        private static TradeBot.Common.v1.Order Convert(FullOrder order)
        {
            return new()
            {
                Id = order.Id,
                LastUpdateDate = new Timestamp
                {
                    Seconds = order.LastUpdateDate.Second
                },
                Price = order.Price,
                Quantity = order.Quantity,
                Signature = order.Signature
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


        public TradeMarketService()
        {
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
            var user = UserContext.GetUserContext(sessionId, slot);

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

            var user = UserContext.GetUserContext(sessionId, slot);

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
                Log.Logger.Warning("Connection was interrupted by network services.");
            }
        }

        public async override Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var user = UserContext.GetUserContext(sessionId,slot);

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

        public async override Task SubscribeMyOrders(SubscribeMyOrdersRequest request, IServerStreamWriter<SubscribeMyOrdersResponse> responseStream, ServerCallContext context)
        {
            Log.Logger.Information($"Connected with {context.Host}");

            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            try
            {
                var user = UserContext.GetUserContext(sessionId, slot);
                user.UserOrders += async (sender, args) => {
                    var order = ConvertOrder(args);
                    Log.Logger.Information($"Sent order : {order} to {context.Host}");
                    await WriteStreamAsync<SubscribeMyOrdersResponse>(responseStream, new()
                    {
                        Changed = Convert(args)
                    }) ;
                };
                //TODO отписка после отмены
                await AwaitCancellation(context.CancellationToken);
            }
            catch (Exception e)
            {
                Log.Logger.Error("Exception happened");
                Log.Logger.Error(e.Message);

                Log.Logger.Error(e.StackTrace);

                context.Status = Status.DefaultCancelled;
                context.ResponseTrailers.Add("sessionid", sessionId);
                context.ResponseTrailers.Add("error", e.Message);
                

            }
        }

        public async override Task SubscribeOrders(SubscribeOrdersRequest request, IServerStreamWriter<SubscribeOrdersResponse> responseStream, ServerCallContext context)
        {
            Log.Logger.Information($"Connected with {context.Host}");

            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            try
            {
                var user = UserContext.GetUserContext(sessionId, slot);
                user.Book25 += async (sender, args) => {
                    var order = ConvertOrder(args);
                    Log.Logger.Information($"Sent order : {order} to {context.Host}");
                    await WriteStreamAsync<SubscribeOrdersResponse>(responseStream, order);
                };
                //TODO отписка после отмены
                await AwaitCancellation(context.CancellationToken);
            }
            catch (Exception e)
            {
                Log.Logger.Error("Exception happened");
                Log.Logger.Error(e.Message);

                Log.Logger.Error(e.StackTrace);

                context.Status = Status.DefaultCancelled;
                context.ResponseTrailers.Add("sessionid", sessionId);
                context.ResponseTrailers.Add("error", e.Message);

            }

        }

        private static bool IsOrderSuitForSignature(FullOrder order, TradeBot.Common.v1.OrderSignature signature)
        {
            bool typeCheck = false;
            bool statusCheck = false;
            if(signature.Status == TradeBot.Common.v1.OrderStatus.Unspecified || order.Signature.Status == signature.Status)
            {
                statusCheck = true;
            }
            if(signature.Type == TradeBot.Common.v1.OrderType.Unspecified || order.Signature.Type == signature.Type)
            {
                typeCheck = true;
            }
            return typeCheck && statusCheck;
        }

        private static Task AwaitCancellation(CancellationToken token)
        {
            var completion = new TaskCompletionSource<object>();
            token.Register(() => completion.SetResult(null));
            return completion.Task;
        }
    }
}
