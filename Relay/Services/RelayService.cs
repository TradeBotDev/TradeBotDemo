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

namespace Relay
{
    class Relay : RelayService.RelayServiceBase
    {
        private readonly ILogger<Relay> _logger;
        public Relay(ILogger<Relay> logger) => _logger = logger;

        private bool IsStarted = true;

        public override Task<StartBotResponse> StartBot(StartBotRequest request, ServerCallContext context)
        {
            StartBotResponse result = new StartBotResponse
            {
                
            };

            return Task.FromResult<StartBotResponse>(result);
        }

        public override Task<TradeBot.Relay.RelayService.v1.UpdateServerConfigResponse> UpdateServerConfig(TradeBot.Relay.RelayService.v1.UpdateServerConfigRequest request, ServerCallContext context)
        {
            return base.UpdateServerConfig(request, context);
        }

        public override Task SubscribeLogs(TradeBot.Relay.RelayService.v1.SubscribeLogsRequest request, IServerStreamWriter<TradeBot.Relay.RelayService.v1.SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            return base.SubscribeLogs(request, responseStream, context);
        }

        public override Task<AddOrderResponse> AddOrder(IAsyncStreamReader<AddOrderRequest> requestStream, ServerCallContext context)
        {
            return base.AddOrder(requestStream, context);
        }

        public override Task SubscribeOrders(TradeBot.Relay.RelayService.v1.SubscribeOrdersRequest request, IServerStreamWriter<TradeBot.Relay.RelayService.v1.SubscribeOrdersResponse> responseStream, ServerCallContext context)
        {
            return base.SubscribeOrders(request, responseStream, context);
        }
    }
}
