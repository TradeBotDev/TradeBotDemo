using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common;
using TradeBot.Common.v1;
using TradeBot.Relay.RelayService.v1;
using TradeBot.Algorithm.AlgorithmService.v1;
using TradeBot.Former.FormerService.v1;

namespace Relay
{
    class Relay : RelayService.RelayServiceBase
    {
        private readonly ILogger<Relay> _logger;
        public Relay(ILogger<Relay> logger) => _logger = logger;

        private bool IsStarted = true;
        private Config config;

        //ДОПИСАН
        public override Task<StartBotResponse> StartBot(StartBotRequest request, ServerCallContext context)
        {
            StartBotResponse result = new StartBotResponse();
            result.Response.Code = ReplyCode.Succeed;
            if (IsStarted) result.Response.Message = "Бот успешно запущен";
            return Task.FromResult<StartBotResponse>(result);
        }

        //ДОПИСАН
        public override Task<TradeBot.Relay.RelayService.v1.UpdateServerConfigResponse> UpdateServerConfig(TradeBot.Relay.RelayService.v1.UpdateServerConfigRequest request,
            ServerCallContext context)
        {
            this.config = request.Request.Config;
            var response = new TradeBot.Relay.RelayService.v1.UpdateServerConfigResponse();
            return Task.FromResult(response);
        }

        public override Task SubscribeLogs(TradeBot.Relay.RelayService.v1.SubscribeLogsRequest request,
            IServerStreamWriter<TradeBot.Relay.RelayService.v1.SubscribeLogsResponse> responseStream,
            ServerCallContext context)
        {
            return base.SubscribeLogs(request, responseStream, context);
        }

        //ДОПИСАН
        public override async Task<TradeBot.Relay.RelayService.v1.AddOrderResponse> AddOrder(IAsyncStreamReader<TradeBot.Relay.RelayService.v1.AddOrderRequest> requestStream,
            ServerCallContext context)
        {
            GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new AlgorithmService.AlgorithmServiceClient(channel);
            var stream = client.AddOrder();
            while (await requestStream.MoveNext())
            {
                var request = new TradeBot.Algorithm.AlgorithmService.v1.AddOrderRequest();
                request.Order = requestStream.Current.Order;
                await stream.RequestStream.WriteAsync(request);
            }
            await stream.RequestStream.CompleteAsync();

            var response = new TradeBot.Relay.RelayService.v1.AddOrderResponse();
            return response;
        }

        public override async Task SubscribeOrders(TradeBot.Relay.RelayService.v1.SubscribeOrdersRequest request, IServerStreamWriter<TradeBot.Relay.RelayService.v1.SubscribeOrdersResponse> responseStream, ServerCallContext context)
        {

            //var tradeMarketClient = new FormerService.FormerServiceClient(Channels.TradeMarketChannel);
            //var orderSignature = new OrderSignature
            //{
            //    Status = OrderStatus.Open,
            //    Type = OrderType.Buy
            //};
            //var request = new SubscribeOrdersRequest()
            //{
            //    Signature = orderSignature
            //};

            //using var call = tradeMarketClient.SubscribeOrders(request);
            //while (await call.ResponseStream.MoveNext())
            //{
            //    Former.UpdateCurrentOrders(call.ResponseStream.Current);
            //}
            //TODO выход из цикла и дальнейшее закрытие канала
        }
    }
}
