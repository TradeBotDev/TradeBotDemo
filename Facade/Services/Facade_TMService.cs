using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TradeBot.Facade.FacadeService.v1;
using static TradeBot.Facade.FacadeService.v1.FacadeService;

namespace Facade
{
    public class FacadeTMService : TradeBot.Facade.FacadeService.v1.FacadeService.FacadeServiceBase
    {
        private FacadeServiceClient clientTM = new FacadeServiceClient(GrpcChannel.ForAddress("https://localhost:5005"));
        private readonly ILogger<FacadeTMService> _logger;
        public FacadeTMService(ILogger<FacadeTMService> logger)
        {
            _logger = logger;
        }

        public override async Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<TradeBot.Facade.FacadeService.v1.SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {
            using var response = clientTM.SubscribeBalance(request);
            while (await response.ResponseStream.MoveNext())
            {
                await responseStream.WriteAsync(new SubscribeBalanceResponse
                {
                    Response = response.ResponseStream.Current.Response
                });
            }
        }

        public override Task<AuthenticateTokenResponse> AuthenticateToken(AuthenticateTokenRequest request, ServerCallContext context)
        {
            var response = clientTM.AuthenticateToken(new AuthenticateTokenRequest { Token = request.Token });

            return Task.FromResult(new AuthenticateTokenResponse
            {
                Response = response.Response
            });
        }

        public override async Task Slots(SlotsRequest request, IServerStreamWriter<SlotsResponse> responseStream, ServerCallContext context)
        {
            using var response = clientTM.Slots(request);

            while (await response.ResponseStream.MoveNext())
            {
                await responseStream.WriteAsync(new SlotsResponse
                {
                    SlotName = response.ResponseStream.Current.SlotName
                });
            }
        }
        public override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request, ServerCallContext context)
        {
            var response = clientTM.UpdateServerConfig(request);

            return Task.FromResult(new UpdateServerConfigResponse
            { 
                Response= response.Response
            });

        }
        public override async Task SubscribeLogs(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            using var response = clientTM.SubscribeLogs(request);
            while(await response.ResponseStream.MoveNext())
            {
                await responseStream.WriteAsync(new SubscribeLogsResponse
                { 
                    Response = response.ResponseStream.Current.Response
                });
            }
        }
    }

}
