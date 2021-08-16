using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rel = TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient;
using Ref = TradeBot.Facade.FacadeService.v1;
using Serilog;
using Grpc.Core;

namespace Facade
{
    public class RelayClass
    {
        private GrpcChannel _channel => GrpcChannel.ForAddress("http://localhost:5004");
        private GrpcChannel Channel { get => _channel; }

        private Rel _client => new Rel(Channel);
        public Rel Client
        {
            get => _client;
        }

        public async Task Relay_SubscribeLogs(Ref.SubscribeLogsRequest request, IServerStreamWriter<Ref.SubscribeLogsResponse> responseStream, ServerCallContext context, string methodName)
        {
            var response = Client.SubscribeLogs(new TradeBot.Relay.RelayService.v1.SubscribeLogsRequest { Request = request.R }, context.RequestHeaders);
            await Generalization.StreamReadWrite(request, responseStream, response, context, methodName);
        }
        public async Task<Ref.DeleteOrderResponse> Relay_DeleteOrder(ServerCallContext context, string methodName)
        {
            TradeBot.Relay.RelayService.v1.DeleteOrderResponse response = null;
            async Task<TradeBot.Relay.RelayService.v1.DeleteOrderResponse> task()
            {
                response = await Client.DeleteOrderAsync(new TradeBot.Relay.RelayService.v1.DeleteOrderRequest { }, context.RequestHeaders);
                return response;
            }
            await Generalization.ConnectionTester<TradeBot.Relay.RelayService.v1.DeleteOrderRequest>(task, methodName);
            return await Generalization.ReturnResponse(new TradeBot.Facade.FacadeService.v1.DeleteOrderResponse { Response = response.Response }, methodName);
        }
        public async Task<Ref.SwitchBotResponse> Relay_StartBot(Ref.SwitchBotRequest request, ServerCallContext context, string methodName)
        {
            TradeBot.Relay.RelayService.v1.StartBotResponse response = null;
            async Task<TradeBot.Relay.RelayService.v1.StartBotResponse> task()
            {
                response = await Client.StartBotAsync(new TradeBot.Relay.RelayService.v1.StartBotRequest
                {
                    Config = request.Config
                }, context.RequestHeaders);
                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);
            return await Generalization.ReturnResponse(new Ref.SwitchBotResponse { Response = response.Response }, methodName);
        }
        public async Task<Ref.StopBotResponse> Relay_stopBot(Ref.StopBotRequest request, ServerCallContext context,string methodName)
        {
            TradeBot.Relay.RelayService.v1.StopBotResponse response = null;
            async Task<TradeBot.Relay.RelayService.v1.StopBotResponse> task()
            {
                response = await Client.StopBotAsync(new TradeBot.Relay.RelayService.v1.StopBotRequest
                {
                    Request = request.Request
                }, context.RequestHeaders);
                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);
            return await Generalization.ReturnResponse(new Ref.StopBotResponse { }, methodName);
        }
        public async Task<Ref.UpdateServerConfigResponse> Relay_UpdateServerConfig(Ref.UpdateServerConfigRequest request, ServerCallContext context, string methodName)
        {
            TradeBot.Relay.RelayService.v1.UpdateServerConfigResponse response = null;
            async Task<TradeBot.Relay.RelayService.v1.UpdateServerConfigResponse> task()
            {
                response = await Client.UpdateServerConfigAsync(new TradeBot.Relay.RelayService.v1.UpdateServerConfigRequest
                {
                    Request = request.Request
                }, context.RequestHeaders);
                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);
            return await Generalization.ReturnResponse(new TradeBot.Facade.FacadeService.v1.UpdateServerConfigResponse { Response = response.Response }, methodName);
        }

    }
}
