using System.Linq;
using System.Threading.Tasks;
using Former.Model;
using Grpc.Core;
using TradeBot.Former.FormerService.v1;

namespace Former.Services
{
    public class ListenerImpl : FormerService.FormerServiceBase
    {
        public override async Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request,
            ServerCallContext context)
        {
            var task = Task.Run(() =>
            {
                var meta = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
                var userContext = Contexts.GetUserContext(meta["sessionid"], meta["trademarket"], meta["slot"], request.Request.Config);
                if (request.Request.Switch) userContext.UnsubscribeStorage();
                else userContext.SubscribeStorageToMarket();
            });
            await task;
            return new UpdateServerConfigResponse();
        }

        public override async Task<DeleteOrderResponse> DeleteOrder(DeleteOrderRequest request, ServerCallContext context)
        {
            var meta = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
            await Contexts.GetUserContext(meta["sessionid"], meta["trademarket"], meta["slot"]).RemoveAllMyOrders();
            return new DeleteOrderResponse();
        }

        public override async Task<SendAlgorithmDecisionResponse> SendAlgorithmDecision(SendAlgorithmDecisionRequest request,
            ServerCallContext context)
        {
            var meta = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
            await Contexts.GetUserContext(meta["sessionid"], meta["trademarket"], meta["slot"]).FormOrder(request.Decision);
            return new SendAlgorithmDecisionResponse();
        }
    }
}
