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
        public GrpcChannel _channel => GrpcChannel.ForAddress("https://localhost:5004");
        public GrpcChannel Channel { get => _channel; }

        private Rel _client => new Rel(Channel);
        public Rel Client
        {
            get => _client;
        }

        public async Task SubscribeLogsRel(Ref.SubscribeLogsRequest request, IServerStreamWriter<Ref.SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    var response = Client.SubscribeLogs(new TradeBot.Relay.RelayService.v1.SubscribeLogsRequest { Request = request.R });
                    if (!context.CancellationToken.IsCancellationRequested)
                    {
                        if (response.ResponseStream.Current != null)
                        {
                            Log.Information($"Function: SubscribeLogsRelay \n args: request={request}");
                            while (await response.ResponseStream.MoveNext())
                            {
                                Log.Information($"Function: SubscribeLogsRelay \n args: response={response.ResponseStream.Current.Response}");
                                await responseStream.WriteAsync(new Ref.SubscribeLogsResponse
                                {
                                    Response = response.ResponseStream.Current.Response
                                });
                            }
                            break;
                        }
                        else
                        {
                            //Log.Information("Trying to reconnect")
                        }
                    }
                    else
                    {
                        Log.Information("Client disconnected");
                        break;
                    }
                }
                catch (RpcException e)
                {
                    Log.Error("Exception" + e);
                }
            }
        }

        public Task<Ref.SwitchBotResponse> SwitchBotRel(Ref.SwitchBotRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;
                    var response = Client.StartBot(new TradeBot.Relay.RelayService.v1.StartBotRequest
                    {
                        Config = request.Config
                    }, context.RequestHeaders);
                    Log.Information($"Function: SwitchBot \n args: request={request}");
                    Log.Information($"Function: SwitchBot \n args: response={response}");
                    return Task.FromResult(new Ref.SwitchBotResponse
                    {
                        Response = response.Response
                    });
                }
                catch (RpcException e)
                {
                    Log.Error("Exception:" + e.Message);
                }
            }
            Log.Information("Client disconnected");
            return Task.FromResult(new Ref.SwitchBotResponse { });
        }

        public Task<Ref.UpdateServerConfigResponse> UpdateServerConfigRel(Ref.UpdateServerConfigRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;
                    var response = Client.UpdateServerConfig(new TradeBot.Relay.RelayService.v1.UpdateServerConfigRequest() { Request = request.Request });
                    Log.Information($"Function: UpdateServerConfig \n args: request={request}");
                    Log.Information($"Function: UpdateServerConfig \n args: response={response}");
                    return Task.FromResult(new Ref.UpdateServerConfigResponse
                    {
                        Response = response.Response
                    });
                }
                catch (RpcException e)
                {
                    Log.Error("Exception:" + e.Message);
                }
            }
            Log.Information("Client disconnected");
            return Task.FromResult(new Ref.UpdateServerConfigResponse { });
        }

    }
}
