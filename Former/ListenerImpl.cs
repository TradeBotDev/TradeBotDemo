using Grpc.Core;

using System.Threading.Tasks;

using TradeBot.Former.FormerService.v1;

namespace Former
{
    public class ListenerImpl : FormerService.FormerServiceBase
    {
        public override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request, ServerCallContext context)
        {
            Clients.GetUserContext(context.RequestHeaders.GetValue("sessionId"), context.RequestHeaders.GetValue("trademarket"), context.RequestHeaders.GetValue("slot")).configuration = request.Request;
            return Task.FromResult(new UpdateServerConfigResponse());
        }
        public override Task<SendPurchasePriceResponse> SendPurchasePrice(SendPurchasePriceRequest request, ServerCallContext context)
        {
            Clients.GetUserContext(context.RequestHeaders.GetValue("sessionId"), context.RequestHeaders.GetValue("trademarket"), context.RequestHeaders.GetValue("slot")).FormPurchaseList(request.PurchasePrice);
            return Task.FromResult(new SendPurchasePriceResponse());
        }
    }
}
