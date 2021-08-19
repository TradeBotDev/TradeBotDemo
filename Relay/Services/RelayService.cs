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
using System.Diagnostics.CodeAnalysis;
using Serilog;

namespace Relay.Services
{
    public class RelayService : TradeBot.Relay.RelayService.v1.RelayService.RelayServiceBase
    {
        private static AlgorithmClient _algorithmClient = null;
        private static TradeMarketClient _tradeMarketClient = null;
        private static FormerClient _formerClient=null;//добавил null
        private static MetaComparer comparer = new MetaComparer();

        private static IDictionary<Metadata, UserContext> contexts = new Dictionary<Metadata, UserContext>(comparer);

        private class MetaComparer : IEqualityComparer<Metadata>
        {
            public bool Equals(Metadata x, Metadata y)
            {
                if (x.Get("sessionid") is null || x.Get("slot") is null || x.Get("trademarket") is null)
                {
                    return false;
                }
                if (y.Get("sessionid") is null || y.Get("slot") is null || y.Get("trademarket") is null)
                {
                    return false;
                }
                var b1 = x.GetValue("sessionid") == y.GetValue("sessionid");
                var b2 = x.GetValue("slot") == y.GetValue("slot");
                var b3 = x.GetValue("trademarket") == y.GetValue("trademarket");
                return  b1 && b2 && b3;
            }

            public int GetHashCode([DisallowNull] Metadata obj)
            {
                return 1;
            }
        }

        private UserContext GetUserContext(Metadata meta)
        {
            //TODO рабтает ли тут MetaComparer
            lock (this)
            {
                if (contexts.ContainsKey(meta))
                {
                    return contexts[meta];
                }
                UserContext newContext = new(meta, _formerClient, _algorithmClient, _tradeMarketClient);
                contexts.Add(meta, newContext);
                return newContext;
            }
        }

        public RelayService(AlgorithmClient algorithm, TradeMarketClient tradeMarket, FormerClient former)
        {
            Log.Information("new RelayService");
            _algorithmClient = algorithm;
            _tradeMarketClient = tradeMarket;
            _formerClient = former;
        }

        public override async Task<StartBotResponse> StartBot(StartBotRequest request, ServerCallContext context)
        {
            var user = GetUserContext(context.RequestHeaders);
            Log.Information("{@ServiceName} StartBot Request Started For user {@context}", "Relay", user);
            user.StatusOfWork();
            user.SubscribeForOrders();
            return await Task.FromResult(new StartBotResponse()
            {
                Response = new DefaultResponse()
                {
                    Message = "Command has been completed",
                    Code = ReplyCode.Succeed
                }
            });
        }

        public async override Task<DeleteOrderResponse> DeleteOrder(DeleteOrderRequest request, ServerCallContext context)
        {
            await _formerClient.SendDeleteOrder(new DeleteOrderRequest {},context);
            return await Task.FromResult(new DeleteOrderResponse { });
        }
        public async override Task<StopBotResponse> StopBot(StopBotRequest request, ServerCallContext context)
        {
            var user = GetUserContext(context.RequestHeaders);
            user.UpdateConfig(new TradeBot.Common.v1.UpdateServerConfigRequest { Config = request.Request.Config, Switch = request.Request.Switch });
            user.StatusOfWork();
            return await Task.FromResult(new StopBotResponse { });
        }
        public async override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request,
            ServerCallContext context)
        {
            var user = GetUserContext(context.RequestHeaders);
            user.UpdateConfig(new TradeBot.Common.v1.UpdateServerConfigRequest { Config=request.Request.Config,Switch=request.Request.Switch });
            return await Task.FromResult(new UpdateServerConfigResponse());
        }

    }
}
