using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.Former.FormerService.v1;
using SubscribeLogsRequest = TradeBot.Former.FormerService.v1.SubscribeLogsRequest;
using SubscribeLogsResponse = TradeBot.Former.FormerService.v1.SubscribeLogsResponse;
using UpdateServerConfigRequest = TradeBot.Former.FormerService.v1.UpdateServerConfigRequest;
using UpdateServerConfigResponse = TradeBot.Former.FormerService.v1.UpdateServerConfigResponse;

namespace Former
{
    public class ListenerImpl : FormerService.FormerServiceBase
    {
        public override async Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request,
            ServerCallContext context)
        {
            var task = Task.Run(() =>
            {
                var meta = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
                var userContext = Clients.GetUserContext(meta["sessionid"], meta["trademarket"], meta["slot"], request.Request.Config);
                if (request.Request.Switch) userContext.UnsubscribeStorage();
            });
            await task;
            return new UpdateServerConfigResponse();
        }

        //public override async Task<DeleteOrderResponse> DeleteOrder(DeleteOrderRequest request, ServerCallContext context)
        //{
            
        //}


        public override async Task<SendAlgorithmDecisionResponse> SendAlgorithmDecision(SendAlgorithmDecisionRequest request,
            ServerCallContext context)
        {
            //в зависимости от числа, присланного алгоритмом производится формирование цены на покупку или на продажу с учётом контекста пользователя
            var meta = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
            await Clients.GetUserContext(meta["sessionid"], meta["trademarket"], meta["slot"]).FormOrder(request.Decision);
            return new SendAlgorithmDecisionResponse();
        }

        public override async Task SubscribeLogs(SubscribeLogsRequest request,
            IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            var task = Task.Run(() =>
            {
                var meta = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
                var userContext = Clients.GetUserContext(meta["sessionid"], meta["trademarket"], meta["slot"]);

                async Task LoggerOnNewLog(string arg1, LogLevel arg2, DateTimeOffset arg3)
                {
                    try
                    {
                        await responseStream.WriteAsync(new SubscribeLogsResponse()
                        {
                            Response = new TradeBot.Common.v1.SubscribeLogsResponse
                            {
                                Level = arg2,
                                LogMessage = arg1,
                                Where = "Former",
                                When = arg3.ToTimestamp()
                            }
                        });
                    }
                    catch
                    {
                        // ignored
                    }
                }
                userContext.Logger.NewLog += LoggerOnNewLog;
                while (!context.CancellationToken.IsCancellationRequested) { }
                userContext.Logger.NewLog -= LoggerOnNewLog;

            });
            await task;
        }
    }
}
