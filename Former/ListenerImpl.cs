using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Former.FormerService.v1;

namespace Former
{
    public class ListenerImpl : FormerService.FormerServiceBase
    {
        public override async Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request,
            ServerCallContext context)
        {
            var meta = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
            Clients.GetUserContext(meta["sessionid"], meta["trademarket"], meta["slot"], request.Request);
            return new UpdateServerConfigResponse();
        }

        public override async Task<SendPurchasePriceResponse> SendPurchasePrice(SendPurchasePriceRequest request,
            ServerCallContext context)
        {
            //� ����������� �� �����, ����������� ���������� ������������ ������������ ���� �� ������� ��� �� ������� � ������ ��������� ������������
            var meta = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
            _ = Clients.GetUserContext(meta["sessionid"], meta["trademarket"], meta["slot"])
                .FormOrder((int)request.PurchasePrice);
            return new SendPurchasePriceResponse();
        }
    }
}
