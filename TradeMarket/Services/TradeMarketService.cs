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
using Bitmex.Client.Websocket.Responses.Positions;
using TradeMarket.Model.TradeMarkets;
using TradeMarket.Model.UserContexts;

namespace TradeMarket.Services
{
    public partial class TradeMarketService : TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceBase
    {
        private UserContextDirector director;

        public TradeMarketService(UserContextDirector director)
        {
            this.director = director;
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

            var user = director.GetUserContext(sessionId, slot, trademarket);
            var response = await user.PlaceOrder(request.Value, request.Price);

            return response;
        }


        public async override Task Slots(SlotsRequest request, IServerStreamWriter<SlotsResponse> responseStream, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var trademarket = context.RequestHeaders.Get("trademarket").Value;


            var user = director.GetUserContext(sessionId, slot, trademarket);

            //нет функционала получения всех слотов по вебсокету
            /*FakeSlotPublisher.GetInstance().Changed += async (sender, args) =>
            {
                await WriteStreamAsync<SlotsResponse>(responseStream, ConvertService.(args.Changed));
            };*/
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

            var user = director.GetUserContext(sessionId, slot, trademarket);

            user.UserBalance += async (sender, args) => {
                await WriteStreamAsync<SubscribeBalanceResponse>(responseStream, new SubscribeBalanceResponse { Response = new() { Balance = ConvertService.ConvertBalance(args.Changed) } });
            };
            //TODO отписка после отмены
            await AwaitCancellation(context.CancellationToken);

        }

        public async override Task SubscribePrice(SubscribePriceRequest request, IServerStreamWriter<SubscribePriceResponse> responseStream, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var trademarket = context.RequestHeaders.Get("trademarket").Value;

            var user = director.GetUserContext(sessionId, slot, trademarket);

            user.InstrumentUpdate += async (sender, args) => {
                await WriteStreamAsync<SubscribePriceResponse>(responseStream, ConvertService.ConvertInstrument(args.Changed,args.Action));
            };
            //TODO отписка после отмены
            await AwaitCancellation(context.CancellationToken);
        }

        public async override Task SubscribeMargin(SubscribeMarginRequest request, IServerStreamWriter<SubscribeMarginResponse> responseStream, ServerCallContext context)
        {
            Log.Logger.Information($"Connected with {context.Host}");

            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var trademarket = context.RequestHeaders.Get("trademarket").Value;


            var user = director.GetUserContext(sessionId, slot, trademarket);
            user.UserMargin += async (sender, args) =>
            {
                var marginResponse = ConvertService.ConvertMargin(args.Changed, args.Action);
                //Log.Logger.Information($"Sent order : {order} to {context.Host}");
                await WriteStreamAsync<SubscribeMarginResponse>(responseStream, marginResponse);
            };
            //TODO отписка после отмены
            await AwaitCancellation(context.CancellationToken);

        }

        public async override Task SubscribePosition(SubscribePositionRequest request, IServerStreamWriter<SubscribePositionResponse> responseStream, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var trademarket = context.RequestHeaders.Get("trademarket").Value;

            var user = director.GetUserContext(sessionId, slot, trademarket);


            user.UserPosition += async (sender, args) => {
                await WriteStreamAsync<SubscribePositionResponse>(responseStream, ConvertService.ConvertPosition(args.Changed,args.Action));
            };
            //TODO отписка после отмены
            await AwaitCancellation(context.CancellationToken);
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
            var user = director.GetUserContext(sessionId, slot, trademarket);
            user.UserOrders += async (sender, args) => {

                var response = ConvertService.ConvertMyOrder(args.Changed, args.Action);
                await WriteStreamAsync<SubscribeMyOrdersResponse>(responseStream, response);
            };
            //TODO отписка после отмены
            await AwaitCancellation(context.CancellationToken);

        }

        public async override Task SubscribeOrders(TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersRequest request, IServerStreamWriter<SubscribeOrdersResponse> responseStream, ServerCallContext context)
        {
            Log.Logger.Information($"Connected with {context.Host}");

            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var trademarket = context.RequestHeaders.Get("trademarket").Value;
            try
            {
                var user = director.GetUserContext(sessionId, slot, trademarket);
                user.Book25 += async (sender, args) => {
                    var response = ConvertService.ConvertBookOrders(args.Changed,args.Action);
                    if (IsOrderSuitForSignature(response.Response.Order.Signature, request.Request.Signature))
                    {
                        await WriteStreamAsync<SubscribeOrdersResponse>(responseStream, response);
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

        private static bool IsOrderSuitForSignature(TradeBot.Common.v1.OrderSignature orderSignature, TradeBot.Common.v1.OrderSignature signature)
        {
            bool typeCheck = false;
            bool statusCheck = false;
            if(signature.Status == TradeBot.Common.v1.OrderStatus.Unspecified || orderSignature.Status == signature.Status)
            {
                statusCheck = true;
            }
            if(signature.Type == TradeBot.Common.v1.OrderType.Unspecified || orderSignature.Type == signature.Type)
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

            var user = director.GetUserContext(sessionId, slot, trademarket);

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

            var user = director.GetUserContext(sessionId, slot, trademarket);
            var response = await user.DeleteOrder(request.OrderId);
            return new()
            {
                Response = response
            };

        }
    }
}
