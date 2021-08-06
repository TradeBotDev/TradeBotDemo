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
        private GrpcChannel _channel => GrpcChannel.ForAddress("https://localhost:5005");
        public GrpcChannel Channel { get => _channel; }

        private TMClient _client => new TMClient(Channel);
        public TMClient Client
        {
            get => _client;
        }

        public async Task SubscribeBalanceTradeMarket(Ref.SubscribeBalanceRequest request, IServerStreamWriter<Ref.SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    var response = Client.SubscribeBalance(new TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceRequest { Request = request.Request });
                    if (!context.CancellationToken.IsCancellationRequested)
                    {
                        if (response.ResponseStream.Current != null)
                        {
                            Log.Information($"Function: SubscribeBalance \n args: request={request}");
                            while (await response.ResponseStream.MoveNext())
                            {
                                Log.Information($"Function: SubscribeBalance \n args: response={response.ResponseStream.Current.Response.Balance}");
                                await responseStream.WriteAsync(new TradeBot.Facade.FacadeService.v1.SubscribeBalanceResponse
                                {
                                    Money = response.ResponseStream.Current.Response.Balance
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
                    Log.Error("Exception" + e.Message);
                }
            }
        }

        public async Task SlotsTradeMarket(Ref.SlotsRequest request, IServerStreamWriter<Ref.SlotsResponse> responseStream, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    var response = Client.Slots(new TradeBot.TradeMarket.TradeMarketService.v1.SlotsRequest { });
                    if (!context.CancellationToken.IsCancellationRequested)
                    {
                        if (response.ResponseStream.Current != null)
                        {
                            Log.Information($"Function: Slots \n args: request={request}");
                            while (await response.ResponseStream.MoveNext())
                            {
                                Log.Information($"Function: Slots \n args: response={response.ResponseStream.Current.SlotName}");

                                await responseStream.WriteAsync(new TradeBot.Facade.FacadeService.v1.SlotsResponse
                                {
                                    SlotName = response.ResponseStream.Current.SlotName
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
                    Log.Error("Exception" + e.Message);
                }
            }
        }

        public async Task SubscribeLogsTradeMarket(Ref.SubscribeLogsRequest request, IServerStreamWriter<Ref.SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    var response = Client.SubscribeLogs(new TradeBot.TradeMarket.TradeMarketService.v1.SubscribeLogsRequest { Request = request.R });
                    if (!context.CancellationToken.IsCancellationRequested)
                    {
                        if (response.ResponseStream.Current != null)
                        {
                            Log.Information($"Function: SubscribeLogsTM \n args: request={request}");
                            while (await response.ResponseStream.MoveNext())
                            {
                                Log.Information($"Function: SubscribeBalance \n args: response={response.ResponseStream.Current.Response}");
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
                    Log.Error("Exception" + e.Message);
                }
            }
        }

        public Task<Ref.AuthenticateTokenResponse> AuthenticationTokenTradeMarket(Ref.AuthenticateTokenRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;
                    var response = Client.AuthenticateToken(new TradeBot.TradeMarket.TradeMarketService.v1.AuthenticateTokenRequest { Token = request.Token });
                    Log.Information($"Function: AuthenticateToken \n args: request={request}");
                    Log.Information($"Function: AuthenticateToken \n args: response={response}");
                    return Task.FromResult(new Ref.AuthenticateTokenResponse { Response = response.Response });
                }
                catch (RpcException e)
                {
                    Log.Information("Exception:" + e.Message);
                }
            }
            return Task.FromResult(new Ref.AuthenticateTokenResponse { });
        }
    }
    public static class MyExtension
    {
        public static AsyncServerStreamingCall<RequestAndResponse.SubscribeBalanceResponse> SubscribeBalance(this TMClient cl, RequestAndResponse.SubscribeBalanceRequest request)
        {
            var response = cl.SubscribeBalance(new RequestAndResponse.SubscribeBalanceRequest { Request = request.Request, SlotName = request.SlotName });
            //if (response =)
            return response;
        }
    }
}
