using Grpc.Core;

using System.Threading.Tasks;

using TradeBot.Former.FormerService.v1;

namespace Former
{
    public class ListenerImpl : FormerService.FormerServiceBase
    {
        public override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request, ServerCallContext context)
        {
            Clients.GetUserContext(context.RequestHeaders.GetValue("sessionid"), context.RequestHeaders.GetValue("trademarket"), context.RequestHeaders.GetValue("slot")).configuration = request.Request;
            return Task.FromResult(new UpdateServerConfigResponse());
        }
        public override Task<SendPurchasePriceResponse> SendPurchasePrice(SendPurchasePriceRequest request, ServerCallContext context)
        {
            if (request.PurchasePrice == 1) Clients.GetUserContext(context.RequestHeaders.GetValue("sessionid"), context.RequestHeaders.GetValue("trademarket"), context.RequestHeaders.GetValue("slot")).FormPurchaseOrder();
            else Clients.GetUserContext(context.RequestHeaders.GetValue("sessionid"), context.RequestHeaders.GetValue("trademarket"), context.RequestHeaders.GetValue("slot")).FormSellOrder();

            return Task.FromResult(new SendPurchasePriceResponse());
        }
    }
}
