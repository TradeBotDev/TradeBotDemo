using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using TradeBot.Facade.FacadeService.v1;

namespace Facade
{
    public class FacadeTMService : TradeBot.Facade.FacadeService.v1.FacadeService.FacadeServiceBase
    {
        
        private TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient clientTM = new TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient(GrpcChannel.ForAddress("https://localhost:5005"));
        private TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient clientRelay = new TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient(GrpcChannel.ForAddress("https://localhost:5004"));
        private readonly ILogger<FacadeTMService> _logger;
        public FacadeTMService(ILogger<FacadeTMService> logger)
        {
            _logger = logger;
        }

        public override async Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    var response = clientTM.SubscribeBalance(new TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceRequest { Request = request.Request });
                    if (!context.CancellationToken.IsCancellationRequested)
                    {
                        if (response.ResponseStream.Current != null)
                        {
                            while (await response.ResponseStream.MoveNext())
                            {
                                await responseStream.WriteAsync(new TradeBot.Facade.FacadeService.v1.SubscribeBalanceResponse
                                {
                                    BalanceOne = response.ResponseStream.Current.BalanceOne,
                                    BalanceTwo = response.ResponseStream.Current.BalanceTwo,
                                });
                            }
                            break;
                        }
                        else
                        {
                            Log.Information("Trying to reconnect...");
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
                    Log.Information("Exception" + e);
                }
            }
        }
        public override Task<AuthenticateTokenResponse> AuthenticateToken(AuthenticateTokenRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;
                    var response = clientTM.AuthenticateToken(new TradeBot.TradeMarket.TradeMarketService.v1.AuthenticateTokenRequest { Token = request.Token });
                    return Task.FromResult(new AuthenticateTokenResponse
                    {
                        Response = response.Response
                    });
                }
                catch (RpcException e)
                {
                    Log.Information("Exception:" + e);
                    
                }
            }
            return Task.FromResult(new AuthenticateTokenResponse { });

        }

        public override async Task Slots(SlotsRequest request, IServerStreamWriter<SlotsResponse> responseStream, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    var response = clientTM.Slots(new TradeBot.TradeMarket.TradeMarketService.v1.SlotsRequest{});
                    if (!context.CancellationToken.IsCancellationRequested)
                    {
                        if (response.ResponseStream.Current != null)
                        {
                            while (await response.ResponseStream.MoveNext())
                            {
                                await responseStream.WriteAsync(new SlotsResponse
                                {
                                    SlotName = response.ResponseStream.Current.SlotName
                                });
                            }
                            break;
                        }
                        else
                        {
                            Log.Information("Trying to reconnect...");
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
                    Log.Information("Exception" + e);
                }
            }
        }
        public override Task<SwitchBotResponse> SwitchBot(SwitchBotRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;

                    var response = clientRelay.StartBot(new TradeBot.Relay.RelayService.v1.StartBotRequest { Config = request.Config });
                    return Task.FromResult(new SwitchBotResponse
                    {
                        Response = response.Response
                    });
                }
                catch (RpcException e)
                {
                    Log.Information("Exception:" + e);
                }
            }
            return Task.FromResult(new SwitchBotResponse { });

        }

        public override async Task SubscribeLogs(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    var response = clientRelay.SubscribeLogs(new TradeBot.Relay.RelayService.v1.SubscribeLogsRequest { Request=request.R});
                    if (!context.CancellationToken.IsCancellationRequested)
                    {
                        if (response.ResponseStream.Current != null)
                        {
                            while (await response.ResponseStream.MoveNext())
                            {
                                await responseStream.WriteAsync(new SubscribeLogsResponse
                                {
                                    Response=response.ResponseStream.Current.Response
                                });
                            }
                            break;
                        }
                        else
                        {
                            Log.Information("Trying to reconnect...");
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
                    Log.Information("Exception" + e);
                }
            }

        }
        public override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;
                    var response = clientRelay.UpdateServerConfig(new TradeBot.Relay.RelayService.v1.UpdateServerConfigRequest() { Request = request.Request });

                    return Task.FromResult(new UpdateServerConfigResponse
                    {
                        Response = response.Response
                    });
                }
                catch (RpcException e)
                {
                    Log.Information("Exception:");
                }
            }
            return Task.FromResult(new UpdateServerConfigResponse { });


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
