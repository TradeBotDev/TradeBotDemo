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
        private static AlgorithmClient _algorithmClient = null;
        private static TradeMarketClient _tradeMarketClient = null;
        private static FormerClient _former;

        private static IDictionary<Metadata, UserContext> contexts = new Dictionary<Metadata, UserContext>();

        public UserContext GetUserContext(Metadata meta)
        {
            //TODO Возможно он тут проверяет по ссылке. так что надо бы сделать Equals
            if (contexts.ContainsKey(meta))
            {
                return contexts[meta];
            }
            UserContext newContext = new(meta, _former, _algorithmClient, _tradeMarketClient);
            contexts.Add(meta,newContext);
            return newContext;
        }

        public RelayService(AlgorithmClient algorithm,TradeMarketClient tradeMarket,FormerClient former)
        {
            _algorithmClient = algorithm;
            _tradeMarketClient = tradeMarket;
            _former = former;
        }

        public override async Task<StartBotResponse> StartBot(StartBotRequest request, ServerCallContext context)
        {
            var user = GetUserContext(context.RequestHeaders);
            user.SubscribeForOrders();
            //_algorithmClient.IsOn = true;
            return await Task.FromResult(new StartBotResponse()
            {
                Response = new DefaultResponse()
                {
                    Message = "Bot was launched",
                    Code = ReplyCode.Succeed
                }
            });
        }

        public async override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request,
            ServerCallContext context)
        {
            var user = GetUserContext(context.RequestHeaders);
            user.UpdateConfig(request.Request.Config);
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
