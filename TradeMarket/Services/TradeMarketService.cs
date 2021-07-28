using Google.Protobuf;
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
using Margin = Bitmex.Client.Websocket.Responses.Margins.Margin;
using Newtonsoft.Json;
using Bitmex.Client.Websocket.Responses;
using TradeBot.Common.v1;
using TradeMarket.DataTransfering.Bitmex;

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
            var trademarket = context.RequestHeaders.Get("trademarket").Value;

            var user = await UserContext.GetUserContextAsync(sessionId, slot,trademarket);
            var response = new PlaceOrderResponse()
            {
                OrderId = "",
                Response = new()
                {
                    Code = TradeBot.Common.v1.ReplyCode.Failure,
                    Message = "Exception Happened"
                }
            };
            try
            {
                response = await user.PlaceOrder(request.Value, request.Price);
                return response;
            }
            catch(Exception e)
            {
                Log.Logger.Error("Exception happened");
                Log.Logger.Error(e.Message);

                Log.Logger.Error(e.StackTrace);
            }
            return response;
            
        }


        public async override Task Slots(SlotsRequest request, IServerStreamWriter<SlotsResponse> responseStream, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var trademarket = context.RequestHeaders.Get("trademarket").Value;


            var user = UserContext.GetUserContextAsync(sessionId, slot,trademarket);

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
            var trademarket = context.RequestHeaders.Get("trademarket").Value;

            var user = await UserContext.GetUserContextAsync(sessionId,slot,trademarket);
            foreach(var balance in user.BalanceCache)
            {
                await WriteStreamAsync<SubscribeBalanceResponse>(responseStream, ConvertBalance(balance));
            }

            user.UserBalance += async (sender, args) => {
                await WriteStreamAsync<SubscribeBalanceResponse>(responseStream, ConvertBalance(args));
            };
            //TODO отписка после отмены
            await AwaitCancellation(context.CancellationToken);

        }

        /// <summary>
        /// Конверт через json пока не нашли способа генерить код конвертации
        /// </summary>
        private TradeBot.TradeMarket.TradeMarketService.v1.Margin Convert(Margin margin)
        {
            return new()
            {
                AvailableMargin = margin.AvailableMargin ?? default(long),
                RealisedPnl = margin.RealisedPnl ?? default(long),
                MarginBalance = margin.MarginBalance ?? default(long)
            };
        }

        private TradeBot.TradeMarket.TradeMarketService.v1.ChangesType Convert(BitmexAction action)
        {
            switch (action)
            {
                case BitmexAction.Undefined: return ChangesType.Undefiend;
                case BitmexAction.Partial: return ChangesType.Partitial;
                case BitmexAction.Insert: return ChangesType.Insert;
                case BitmexAction.Update:return ChangesType.Update;
                case BitmexAction.Delete: return ChangesType.Delete;
            }
            return ChangesType.Undefiend;
        }

        public async override Task SubscribeMargin(SubscribeMarginRequest request, IServerStreamWriter<SubscribeMarginResponse> responseStream, ServerCallContext context)
        {
            Log.Logger.Information($"Connected with {context.Host}");

            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var trademarket = context.RequestHeaders.Get("trademarket").Value;

            try
            {
                var user = await UserContext.GetUserContextAsync(sessionId, slot, trademarket);
                user.UserMargin += async (sender, args) => {
                    var margin = Convert(args.Changed);
                    //Log.Logger.Information($"Sent order : {order} to {context.Host}");
                    await WriteStreamAsync<SubscribeMarginResponse>(responseStream, new()
                    {
                        ChangedType = Convert(args.Action),
                        Margin = margin

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

        public override Task SubscribeLogs(TradeBot.TradeMarket.TradeMarketService.v1.SubscribeLogsRequest request, IServerStreamWriter<TradeBot.TradeMarket.TradeMarketService.v1.SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            return base.SubscribeLogs(request, responseStream, context);
        }

        public async override Task SubscribeMyOrders(SubscribeMyOrdersRequest request, IServerStreamWriter<SubscribeMyOrdersResponse> responseStream, ServerCallContext context)
        {
            Log.Logger.Information($"Connected with {context.Host}");

            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var trademarket = context.RequestHeaders.Get("trademarket").Value;

            try
            {
                var user = await UserContext.GetUserContextAsync(sessionId, slot, trademarket);
                user.UserOrders += async (sender, args) => {
                    var defaultResponse = new DefaultResponse()
                    {
                        Code = string.IsNullOrEmpty(args.OrdRejReason) ? ReplyCode.Succeed : ReplyCode.Failure,
                        Message = string.IsNullOrEmpty(args.OrdRejReason) ? args.OrdRejReason! : ""
                    };
                    Order order = Convert(args);
                    Log.Logger.Information($"Sent order : {order} to {context.Host}");
                    await WriteStreamAsync<SubscribeMyOrdersResponse>(responseStream, new()
                    {
                        Response = defaultResponse,
                        Changed = order
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

        private Order Convert(Bitmex.Client.Websocket.Responses.Orders.Order args)
        {
            return new()
            {
                Id = args.OrderId,
                LastUpdateDate = new Timestamp
                {
                    Seconds = args.Timestamp.Value.Second
                },

                Price = (int)(args.Price ?? default(int)),
                Quantity = (int)(args.OrderQty ?? default(int)),
                Signature = new OrderSignature()
                {
                    Status = args.OrdStatus == Bitmex.Client.Websocket.Responses.Orders.OrderStatus.Filled ? OrderStatus.Closed : OrderStatus.Open,
                    Type = args.Side == BitmexSide.Buy ? OrderType.Buy : OrderType.Sell
                }
            };
        }

        public async override Task SubscribeOrders(TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersRequest request, IServerStreamWriter<SubscribeOrdersResponse> responseStream, ServerCallContext context)
        {
            Log.Logger.Information($"Connected with {context.Host}");

            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var trademarket = context.RequestHeaders.Get("trademarket").Value;
            try
            {
                var user = await UserContext .GetUserContextAsync(sessionId, slot, trademarket);
                user.Book25 += async (sender, args) => {
                    var order = ConvertOrder(args);
                    if (IsOrderSuitForSignature(args, request.Request.Signature))
                    {
                        await WriteStreamAsync<SubscribeOrdersResponse>(responseStream, order);
                    }
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

        public async override Task<AmmendOrderResponse> AmmendOrder(AmmendOrderRequest request, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var trademarket = context.RequestHeaders.Get("trademarket").Value;

            var user = await UserContext.GetUserContextAsync(sessionId, slot, trademarket);

            double? price = 0;
            switch (request.PriceType)
            {
                case PriceType.Default:     price = request.NewPrice;break;
                case PriceType.None:        price = null;break;
                case PriceType.Unspecified: throw new RpcException(Status.DefaultCancelled,$"{nameof(request.PriceType)} should be specified");
            }
            long? quantity = null, leavesQuantity = null;
            switch (request.QuantityType)
            {
                case QuantityType.Leaves:       leavesQuantity = request.NewQuantity;break;
                case QuantityType.Default:      quantity = request.NewQuantity;break;
                case QuantityType.None:         break;
                case QuantityType.Unspecified:  throw new RpcException(Status.DefaultCancelled, $"{nameof(request.QuantityType)} should be specified");
            }
            var response = await user.AmmendOrder(request.Id,price,quantity,leavesQuantity);

            return new()
            {
                Response = response
            };
        }

        public async override Task<DeleteOrderResponse> DeleteOrder(DeleteOrderRequest request, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var trademarket = context.RequestHeaders.Get("trademarket").Value;

            var user = await UserContext.GetUserContextAsync(sessionId, slot, trademarket);
            var response = await user.DeleteOrder(request.OrderId);
            return new()
            {
                Response = response
            };

        }
    }
}
