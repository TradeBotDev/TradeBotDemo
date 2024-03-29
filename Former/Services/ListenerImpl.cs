using Former.Models;
using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;
using Former.Clients;
using Serilog;
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
                var metadata = Converters.ConvertMetadata(context.RequestHeaders);

                //�� ����� �������� ����������, �� ������� �������� �������� 
                var userContext = Contexts.GetUserContext(metadata.Sessionid, metadata.Trademarket, metadata.Slot, metadata.UserId);
                Meta.GetMetadata(metadata.Sessionid, metadata.Trademarket, metadata.Slot, metadata.UserId);

                //���� ���� Switch ������������ � false, ������ �� �������� ������ (��� ���������� �), ���� ����������� � true
                //�� ���������� ���������� ������ �������, �� ���� ���������� �� ������������.
                if (request.Request.Switch)
                {
                    userContext.UnsubscribeStorage();
                    RedisClient.DeleteMetaEntries();
                    RedisClient.DeleteConfigurations(Meta.GetMetaList());
                    Contexts.RemoveContext(metadata);
                }
                else
                {
                    await RedisClient.WriteMetaEntries(Meta.GetMetaList());
                    await RedisClient.WriteConfiguration(metadata, Converters.ConvertConfiguration(request.Request.Config));
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
            await Contexts.GetUserContext(meta["sessionid"], meta["trademarket"], meta["slot"], meta["userid"]).RemoveAllMyOrders();
            return new DeleteOrderResponse();
        }

        /// <summary>
        /// ��������� ������� �� ���������, �� �������� ���������� ����� ��������� �����.
        /// </summary>
        public override async Task<SendAlgorithmDecisionResponse> SendAlgorithmDecision(SendAlgorithmDecisionRequest request,
            ServerCallContext context)
        {
            var meta = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
            await Contexts.GetUserContext(meta["sessionid"], meta["trademarket"], meta["slot"], meta["userid"]).FormOrder(request.Decision);
            return new SendAlgorithmDecisionResponse();
        }
    }
}
