using Grpc.Core;

using System.Threading.Tasks;

using TradeBot.Former.FormerService.v1;

namespace Former
{
    public class ListenerImpl : FormerService.FormerServiceBase
    {
        private readonly Former _former;

        public ListenerImpl(Former former)
        {
            _former = former;
        }

        public override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request, ServerCallContext context)
        {
            if (!UserContextFactory.Contains(context.RequestHeaders.Get("sessionId").Value)) UserContextFactory.AddUserContext
                (
                    context.RequestHeaders.GetValue("sessionId"),
                    context.RequestHeaders.GetValue("trademarket"),
                    context.RequestHeaders.GetValue("slot"),
                    request.Request
                );
            return Task.FromResult(new UpdateServerConfigResponse());
        }
        public override Task<SendPurchasePriceResponse> SendPurchasePrice(SendPurchasePriceRequest request, ServerCallContext context)
        {
            UserContextFactory.GetUserContextById(context.RequestHeaders.GetValue("sessionId")).former.FormPurchaseList(request.PurchasePrice);
            return Task.FromResult(new SendPurchasePriceResponse());
        }
    }
}
