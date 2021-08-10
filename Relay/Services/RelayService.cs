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

        private static IDictionary<Metadata, UserContext> contexts = new Dictionary<Metadata, UserContext>(new MetaComparer());

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
                return x.Get("sessionid") == y.Get("sessionid") && x.Get("slot") == y.Get("slot") && x.Get("trademarket") == y.Get("trademarket");
            }

            public int GetHashCode([DisallowNull] Metadata obj)
            {
                return obj.GetHashCode();
            }
        }

        private UserContext GetUserContext(Metadata meta)
        {
            //TODO Возможно он тут проверяет по ссылке. так что надо бы сделать Equals
            if (contexts.Keys.FirstOrDefault(x => x[2].Value == meta[2].Value) != null)
            {
                return contexts.First(x => x.Value.Meta[2].Value == meta[2].Value).Value;
            }
            UserContext newContext = new(meta, _formerClient, _algorithmClient, _tradeMarketClient);
            contexts.Add(meta, newContext);
            return newContext;
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
            user.StatusOfWork();
                user.SubscribeForOrders();
            //user.UpdateConfig(new TradeBot.Common.v1.UpdateServerConfigRequest { Config=request.Config });

            return await Task.FromResult(new StartBotResponse()
            {
                Response = new DefaultResponse()
                {
                    Message = "Command has been complited",
                    Code = ReplyCode.Succeed
                }
            });
        }

        public async override Task<DeleteOrderResponse> DeleteOrder(DeleteOrderRequest request, ServerCallContext context)
        {
            await _formerClient.SendDeleteOrder(new DeleteOrderRequest {},context);
            return await Task.FromResult(new DeleteOrderResponse { });
        }

        public async override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request,
            ServerCallContext context)
        {
            var user = GetUserContext(context.RequestHeaders);
            user.StatusOfWork();
            user.UpdateConfig(new TradeBot.Common.v1.UpdateServerConfigRequest { Config=request.Request.Config,Switch=request.Request.Switch });
            return await Task.FromResult(new UpdateServerConfigResponse());
        }

    }
}
