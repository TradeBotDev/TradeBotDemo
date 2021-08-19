using Former.Models;
using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;
using Former.Clients;
using TradeBot.Former.FormerService.v1;

namespace Former.Services
{
    public class ListenerImpl : FormerService.FormerServiceBase
    {
        /// <summary>
        /// ������ ����� ��������� ������ �������, ��� ��� ����������� ������, �� ���� ������ �������� � ��������� ��������.
        /// ����� � ���������� ����� ������� �� ��������� ���� ��������� ����������� ������������.
        /// </summary>
        public override async Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request,
            ServerCallContext context)
        {
            var task = Task.Run(async () =>
            {
                var meta = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
                //�� ����� �������� ����������, �� ������� �������� �������� 
                var userContext = Contexts.GetUserContext(meta["sessionid"], meta["trademarket"], meta["slot"]);
                //���� ���� Switch ������������ � false, ������ �� �������� ������ (��� ���������� �), ���� ����������� � true
                //�� ���������� ���������� ������ �������, �� ���� ���������� �� ������������.
                Meta.GetMetadata(meta["sessionid"], meta["trademarket"], meta["slot"]);

                if (request.Request.Switch)
                {
                    userContext.UnsubscribeStorage();
                    await RedisClient.DeleteMetaEntries();
                    await RedisClient.DeleteConfigurations(Meta.GetMetaList());
                }
                else
                {
                    await RedisClient.WriteMeta(Meta.GetMetaList());
                    //������������� ��� ��������� ������������
                    userContext.SetConfiguration(Converters.ConvertConfiguration(request.Request.Config));
                    //������������� �� �����������
                    userContext.SubscribeStorageToMarket();
                }
            });
            await task;
            return new UpdateServerConfigResponse();
        }

        /// <summary>
        /// ��������� ������ �� ����� �� �������� ���� ����� ������� (������ ����������� �� ������ � UI).
        /// </summary>
        public override async Task<DeleteOrderResponse> DeleteOrder(DeleteOrderRequest request, ServerCallContext context)
        {
            var meta = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
            await Contexts.GetUserContext(meta["sessionid"], meta["trademarket"], meta["slot"]).RemoveAllMyOrders();
            return new DeleteOrderResponse();
        }

        /// <summary>
        /// ��������� ������� �� ���������, �� �������� ���������� ����� ��������� �����.
        /// </summary>
        public override async Task<SendAlgorithmDecisionResponse> SendAlgorithmDecision(SendAlgorithmDecisionRequest request,
            ServerCallContext context)
        {
            var meta = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
            await Contexts.GetUserContext(meta["sessionid"], meta["trademarket"], meta["slot"]).FormOrder(request.Decision);
            return new SendAlgorithmDecisionResponse();
        }
    }
}
