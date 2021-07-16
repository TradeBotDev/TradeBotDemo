using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Relay.Model;
using TradeBot.Relay.RelayService.v1;
using Relay.Clients;
using TradeBot.Common.v1;
using SubscribeLogsRequest = TradeBot.Relay.RelayService.v1.SubscribeLogsRequest;
using SubscribeLogsResponse = TradeBot.Relay.RelayService.v1.SubscribeLogsResponse;
using UpdateServerConfigRequest = TradeBot.Relay.RelayService.v1.UpdateServerConfigRequest;
using UpdateServerConfigResponse = TradeBot.Relay.RelayService.v1.UpdateServerConfigResponse;

namespace Relay.Services
{
    public class RelayService : TradeBot.Relay.RelayService.v1.RelayService.RelayServiceBase
    {
        private AlgorithmClientService _algorithmClient = null;
        private TradeMarketClientService _tradeMarketClient = null;

        public RelayService(AlgorithmClientService algorithm,TradeMarketClientService tradeMarket)
        {
            _algorithmClient = algorithm;
            _tradeMarketClient = tradeMarket;
        }

        public override Task<StartBotResponse> StartBot(StartBotRequest request, ServerCallContext context)
        {
            _algorithmClient.IsOn = true;
            _tradeMarketClient.ReadOrders().Start();
            return Task.FromResult(new StartBotResponse()
            {
                Response = new DefaultResponse()
                {
                    Message = "Bot has been started",
                    Code = ReplyCode.Succeed
                }
            });
        }

        public async override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request,
            ServerCallContext context)
        {
            await _algorithmClient.UpdateConfig(request.Request.Config);
            return await Task.FromResult(new UpdateServerConfigResponse());
        }

        public override Task SubscribeLogs(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            return base.SubscribeLogs(request, responseStream, context);
        }
        //public async override Task SubscribeOrders(TradeBot.Relay.RelayService.v1.SubscribeOrdersRequest request, IServerStreamWriter<TradeBot.Relay.RelayService.v1.SubscribeOrdersResponse> responseStream, ServerCallContext context)
        //{
           
        //}
    }
}
