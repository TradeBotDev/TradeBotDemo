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
            return Task.FromResult(new UpdateServerConfigResponse());
        }
        public override Task<SendPurchasePriceResponse> SendPurchasePrice(SendPurchasePriceRequest request, ServerCallContext context)
        {
            _former.FormShoppingList(request.PurchasePrice);
            return Task.FromResult(new SendPurchasePriceResponse());
        }
    }
}
