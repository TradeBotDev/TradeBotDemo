using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RequestAndResponse = TradeBot.TradeMarket.TradeMarketService.v1;
using Ref = TradeBot.Facade.FacadeService.v1;
using TMClient = TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient;

namespace Facade
{
    public class TradeMarketClass
    {
        private GrpcChannel _channel => GrpcChannel.ForAddress("http://localhost:5005");
        public GrpcChannel Channel { get => _channel; }

        private TMClient _client => new TMClient(Channel);
        public TMClient Client
        {
            get => _client;
        }

        public async Task TM_SubscribeBalance(Ref.SubscribeBalanceRequest request, IServerStreamWriter<Ref.SubscribeBalanceResponse> responseStream, ServerCallContext context, string methodName)
        {
            var response = Client.SubscribeBalance(new TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceRequest { Request = request.Request });
            await Generalization.StreamReadWrite(request, responseStream, response, context, methodName);
        }

        public async Task TM_Slots(Ref.SlotsRequest request, IServerStreamWriter<Ref.SlotsResponse> responseStream, ServerCallContext context,string methodName)
        {
            var response = Client.Slots(new TradeBot.TradeMarket.TradeMarketService.v1.SlotsRequest { });
            await Generalization.StreamReadWrite(request, responseStream, response, context, methodName);
        }
        public async Task TM_SubscribeLogsTM(Ref.SubscribeLogsRequest request, IServerStreamWriter<Ref.SubscribeLogsResponse> responseStream, ServerCallContext context, string methodName)
        {
            var response = Client.SubscribeLogs(new TradeBot.TradeMarket.TradeMarketService.v1.SubscribeLogsRequest { Request = request.R });
            await Generalization.StreamReadWrite(request, responseStream, response, context, methodName);
        }

        public async Task<Ref.AuthenticateTokenResponse> TM_AuthenticateToken(Ref.AuthenticateTokenRequest request, string methodName)
        {
            TradeBot.TradeMarket.TradeMarketService.v1.AuthenticateTokenResponse response = null;
            async Task<TradeBot.TradeMarket.TradeMarketService.v1.AuthenticateTokenResponse> task()
            {
                response = await Client.AuthenticateTokenAsync(new TradeBot.TradeMarket.TradeMarketService.v1.AuthenticateTokenRequest { Token = request.Token });
                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);
            return await Generalization.ReturnResponse(new Ref.AuthenticateTokenResponse { Response = response.Response }, methodName);
        }
    }
}