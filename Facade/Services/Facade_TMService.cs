using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TradeMarket.Facade.UserInfoService.v1;
using static TradeMarket.Facade.UserInfoService.v1.UserInfoService;

namespace Facade
{
    public class FacadeTMService : TradeMarket.Facade.UserInfoService.v1.UserInfoService.UserInfoServiceBase
    {
        private UserInfoServiceClient clientTM = new UserInfoServiceClient(GrpcChannel.ForAddress("https://localhost:5005"));
        private readonly ILogger<FacadeTMService> _logger;
        public FacadeTMService(ILogger<FacadeTMService> logger)
        {
            _logger = logger;
        }

        public override async Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {
            using var response = clientTM.SubscribeBalance(request);
            while (await response.ResponseStream.MoveNext())
            {
                await responseStream.WriteAsync(new SubscribeBalanceResponse
                {
                    Currency = response.ResponseStream.Current.Currency,
                    Value = response.ResponseStream.Current.Value
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
    }

}
