using Grpc.Core;

using System.Threading.Tasks;

using TradeBot.Former.FormerService.v1;

namespace Former
{
    public class ListenerImpl : FormerService.FormerServiceBase
    {
        public override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request, ServerCallContext context)
        {
            //получаем конфиг от рилея, именно здесь создастся контекст пользователя
            Clients.GetUserContext(context.RequestHeaders.GetValue("sessionid"), context.RequestHeaders.GetValue("trademarket"), context.RequestHeaders.GetValue("slot")).Configuration = request.Request;
            return Task.FromResult(new UpdateServerConfigResponse());
        }
        public override Task<SendPurchasePriceResponse> SendPurchasePrice(SendPurchasePriceRequest request, ServerCallContext context)
        {
            //в зависимости от числа, присланного алгоритмом производится формирование цены на покупку или на продажу с учётом контекста пользователя
            if ((int)request.PurchasePrice == 1) Clients.GetUserContext(context.RequestHeaders.GetValue("sessionid"), context.RequestHeaders.GetValue("trademarket"), context.RequestHeaders.GetValue("slot")).FormBuyOrder();
            else Clients.GetUserContext(context.RequestHeaders.GetValue("sessionid"), context.RequestHeaders.GetValue("trademarket"), context.RequestHeaders.GetValue("slot")).FormSellOrder();
            return Task.FromResult(new SendPurchasePriceResponse());
        }
    }
}
