using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TradeBot.Facade.FacadeService.v1;

namespace Facade
{
    public class FacadeTMService : TradeBot.Facade.FacadeService.v1.FacadeService.FacadeServiceBase
    {
        /*
            Ã≈“Œƒ€ œ≈–≈œ»—¿“‹!!!!!!
         */
        private TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient clientTM = new TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient(GrpcChannel.ForAddress("https://localhost:5005"));
        private TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient clientRelay = new TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient(GrpcChannel.ForAddress("https://localhost:5004"));
        private readonly ILogger<FacadeTMService> _logger;
        public FacadeTMService(ILogger<FacadeTMService> logger)
        {
            _logger = logger;
        }

        public override async Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {
            #region ReconnectionVersion
            //Û‰‡ÎËÚ¸ Ë Ò‰ÂÎ‡¸ ÌÓÏ‡Î¸ÌÓ!!!!
            //AsyncServerStreamingCall<TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceResponse> response = null;
            //for (int i = 0; i < 4; i++)
            //{
            //    try
            //    {
            //        response = clientTM.SubscribeBalance(new TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceRequest { Request = request.Request });
            //        while (await response.ResponseStream.MoveNext())
            //        {
            //            await responseStream.WriteAsync(new TradeBot.Facade.FacadeService.v1.SubscribeBalanceResponse
            //            {
            //                BalanceOne = response.ResponseStream.Current.BalanceOne,
            //                BalanceTwo = response.ResponseStream.Current.BalanceTwo,
            //            });
            //        }

            //        break;
            //    }
            //    catch (Exception ex)
            //    {
            //        if (i == 0)
            //        {
            //            _logger.LogError("The server is not responding.");
            //        }
            //        _logger.LogError("Reconnection attempt... try " + i + "/3");
            //        Thread.Sleep(1000);
            //        if (i == 3)
            //        {
            //            _logger.LogError("timeout exceeded");
            //            _logger.LogError("Exception:\n" + ex);
            //            var defaultResponse = new TradeBot.Common.v1.DefaultResponse
            //            {
            //                Code = TradeBot.Common.v1.ReplyCode.Failure,
            //                Message = "Exception. The server doesnt answer"
            //            };
            //            _ = responseStream.WriteAsync(new TradeBot.Facade.FacadeService.v1.SubscribeBalanceResponse()
            //            {
            //                Message = defaultResponse
            //            });
            //        }
            //    }
            //}
            #endregion
            try
            {
                var response = clientTM.SubscribeBalance(new TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceRequest { Request = request.Request, SlotName=request.SlotName });
                
                while(await response.ResponseStream.MoveNext())
                {
                    await responseStream.WriteAsync(new SubscribeBalanceResponse()
                    {
                        BalanceOne = response.ResponseStream.Current.BalanceOne,
                        BalanceTwo = response.ResponseStream.Current.BalanceTwo
                    });
                }
            }
            catch (RpcException e)
            {
                _logger.LogError("Exception: " + e.Status.DebugException.Message);
                var defaultResponse = new TradeBot.Common.v1.DefaultResponse
                {
                    Code = TradeBot.Common.v1.ReplyCode.Failure,
                    Message = "Exception. The server doesnt answer"
                };
                await responseStream.WriteAsync(new SubscribeBalanceResponse()
                {
                    Message=defaultResponse
                });
            }
        }
        public override Task<AuthenticateTokenResponse> AuthenticateToken(AuthenticateTokenRequest request, ServerCallContext context)
        {
            try
            {
                var response = clientTM.AuthenticateToken(new TradeBot.TradeMarket.TradeMarketService.v1.AuthenticateTokenRequest { Token = request.Token });
                return Task.FromResult(new AuthenticateTokenResponse
                {
                    //Response = new TradeBot.Common.v1.DefaultResponse { Code=TradeBot.Common.v1.ReplyCode.Succeed,Message="otvet"}
                    Response = response.Response
                });
            }
            catch (RpcException e)
            {
                _logger.LogError("Exception: " + e.Status.DebugException.Message);
                var defaultResponse = new TradeBot.Common.v1.DefaultResponse
                {
                    Code = TradeBot.Common.v1.ReplyCode.Failure,
                    Message = "Exception. The server doesnt answer"
                };
                return Task.FromResult(new AuthenticateTokenResponse
                {
                    Message = defaultResponse
                });
            }
        }

        public override async Task Slots(SlotsRequest request, IServerStreamWriter<SlotsResponse> responseStream, ServerCallContext context)
        {
            try
            {
                var response = clientTM.Slots(new TradeBot.TradeMarket.TradeMarketService.v1.SlotsRequest { Empty = request.Empty });

                while (await response.ResponseStream.MoveNext())
                {
                    await responseStream.WriteAsync(new SlotsResponse
                    {
                        SlotName = response.ResponseStream.Current.SlotName
                    });
                }
            }
            catch (RpcException e)
            {
                _logger.LogError("Exception: " + e.Status.DebugException.Message);
                var defaultResponse = new TradeBot.Common.v1.DefaultResponse
                {
                    Code = TradeBot.Common.v1.ReplyCode.Failure,
                    Message = "Exception. The server doesnt answer"
                };
                await responseStream.WriteAsync(new SlotsResponse
                {
                    Message = defaultResponse
                });
            }


        }


        public override Task<SwitchBotResponse> SwitchBot(SwitchBotRequest request, ServerCallContext context)
        {
            try
            {

                var response = clientRelay.StartBot(new TradeBot.Relay.RelayService.v1.StartBotRequest { Config = request.Config });

                return Task.FromResult(new SwitchBotResponse
                {
                    Response = response.Response
                });
            }
            catch (RpcException e)
            {
                _logger.LogError("Exception: " + e.Message);
                var defaultResponse = new TradeBot.Common.v1.DefaultResponse
                {
                    Code = TradeBot.Common.v1.ReplyCode.Failure,
                    Message = "Exception"
                };
                return Task.FromResult(new SwitchBotResponse
                {
                    Response = defaultResponse
                });
            }

        }

        public override async Task SubscribeLogs(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            try
            {
                var response = clientRelay.SubscribeLogs(new TradeBot.Relay.RelayService.v1.SubscribeLogsRequest { Request = request.R }, cancellationToken: context.CancellationToken);

                while (await response.ResponseStream.MoveNext())
                {
                    //ÚÓÍÂÌ Ì‡ ˝ÍÒÂÔ¯ÂÌ
                    context.CancellationToken.ThrowIfCancellationRequested();
                    await responseStream.WriteAsync(new SubscribeLogsResponse
                    {
                        Response = response.ResponseStream.Current.Response
                    });
                }
            }
            catch (RpcException e)
            {
                _logger.LogError("Exception: " + e.Message);
                var defaultResponse = new TradeBot.Common.v1.DefaultResponse
                {
                    Code = TradeBot.Common.v1.ReplyCode.Failure,
                    Message = "Exception"
                };
                await responseStream.WriteAsync(new SubscribeLogsResponse
                {
                    Message = defaultResponse
                });
            }

        }
        public override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request, ServerCallContext context)
        {
            try
            {
                var response = clientRelay.UpdateServerConfig(new TradeBot.Relay.RelayService.v1.UpdateServerConfigRequest() { Request = request.Request });


                return Task.FromResult(new UpdateServerConfigResponse
                {
                    Response = response.Response
                });
            }
            catch (RpcException e)
            {
                _logger.LogError("Exception: " + e.Message);
                var defaultResponse = new TradeBot.Common.v1.DefaultResponse
                {
                    Code = TradeBot.Common.v1.ReplyCode.Failure,
                    Message = "Exception"
                };
                return Task.FromResult(new UpdateServerConfigResponse
                {
                    Message = defaultResponse
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
