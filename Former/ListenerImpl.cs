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
            Former.config = request.Request;
            return Task.FromResult(new UpdateServerConfigResponse());
        }
        public override Task<SendPurchasePriceResponse> SendPurchasePrice(SendPurchasePriceRequest request, ServerCallContext context)
        {
            var meta = new Metadata()
            {
                context.RequestHeaders.Get("sessionId"),
                context.RequestHeaders.Get("trademarket"),
                context.RequestHeaders.Get("slot")
            };
            TradeMarketClient.Configure("https://localhost:5005", 10000, meta);
            _former.FormPurchaseList(request.PurchasePrice);
            return Task.FromResult(new SendPurchasePriceResponse());
        }
    }
}
