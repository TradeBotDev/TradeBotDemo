using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TradeBot.Facade.FacadeService.v1;

namespace Facade
{
    public class FacadeTMService : FacadeService.FacadeServiceBase
    {
        private TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient clientTM = new TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient(GrpcChannel.ForAddress("https://localhost:5005"));
        private readonly ILogger<FacadeTMService> _logger;
        public FacadeTMService(ILogger<FacadeTMService> logger)
        {
            _logger = logger;
        }
        
        public override async Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {
            using var response = clientTM.SubscribeBalance(new TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceRequest {Request=request.Request });
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
            System.Console.WriteLine("Вызов метода AuthenticateToken спараметром: " + request.Token);

            try
            {
                var response = clientTM.AuthenticateToken(new TradeBot.TradeMarket.TradeMarketService.v1.AuthenticateTokenRequest {Token=request.Token });

                System.Console.WriteLine("Возврат значения из AuthenticateToken:" + response.Response.ToString());
                return Task.FromResult(new AuthenticateTokenResponse
                {
                    Response = response.Response
                });
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Ошибка работы метода AuthenticateToken");
                System.Console.WriteLine("Exception: " + e.Message);
                var defaultResponse = new TradeBot.Common.v1.DefaultResponse
                {
                    Code = TradeBot.Common.v1.ReplyCode.Failure,
                    Message = "Exception"
                };
                return Task.FromResult(new AuthenticateTokenResponse
                {
                    Response = defaultResponse
                });
            }
            
        }

        public override async Task Slots(SlotsRequest request, IServerStreamWriter<SlotsResponse> responseStream, ServerCallContext context)
        {
            using var response = clientTM.Slots(new TradeBot.TradeMarket.TradeMarketService.v1.SlotsRequest { Empty=request.Empty});

            while (await response.ResponseStream.MoveNext())
            {
                await responseStream.WriteAsync(new SlotsResponse
                {
                    SlotName = response.ResponseStream.Current.SlotName
                });
            }
        }
        //public override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request, ServerCallContext context)
        //{
        //    var response = clientTM.UpdateServerConfig(request);

        //    return Task.FromResult(new UpdateServerConfigResponse
        //    { 
        //        Response= response.Response
        //    });

        //}
        //public override async Task SubscribeLogs(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        //{
        //    using var response = clientTM.SubscribeLogs(request);
        //    while(await response.ResponseStream.MoveNext())
        //    {
        //        await responseStream.WriteAsync(new SubscribeLogsResponse
        //        { 
        //            Response = response.ResponseStream.Current.Response
        //        });
        //    }
        //}
    }

}
