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
        private TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient clientTM = new TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient(GrpcChannel.ForAddress("https://localhost:5005"));
        private TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient clientRelay = new TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient(GrpcChannel.ForAddress("https://localhost:5004"));
        private readonly ILogger<FacadeTMService> _logger;
        public FacadeTMService(ILogger<FacadeTMService> logger)
        {
            _logger = logger;
        }

        public override async Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {
            AsyncServerStreamingCall<TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceResponse> response = null;
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    response = clientTM.SubscribeBalance(new TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceRequest { Request = request.Request });
                    while (await response.ResponseStream.MoveNext())
                    {
                        await responseStream.WriteAsync(new TradeBot.Facade.FacadeService.v1.SubscribeBalanceResponse
                        {
                            BalanceOne = response.ResponseStream.Current.BalanceOne,
                            BalanceTwo = response.ResponseStream.Current.BalanceTwo,
                        });
                    }
                    //await responseStream.WriteAsync(new TradeBot.Facade.FacadeService.v1.SubscribeBalanceResponse()
                    //{
                    //    BalanceOne = new TradeBot.Common.v1.Balance { Currency = "11", Value = "22" },
                    //    BalanceTwo = new TradeBot.Common.v1.Balance { Currency = "33", Value = "44" }
                    //});
                    break;
                }
                catch (Exception ex)
                {
                    if (i == 0)
                    {
                        _logger.LogWarning("The server is not responding.\n");
                    }
                    _logger.LogWarning("Reconnection attempt... try " + i + "/3");
                    Thread.Sleep(1000);
                    if (i == 3)
                    {
                        _logger.LogWarning("timeout exceeded");
                        _logger.LogWarning("Exception:\n" + ex);
                        _ = responseStream.WriteAsync(new TradeBot.Facade.FacadeService.v1.SubscribeBalanceResponse()
                        {
                            Message = "Exception. The server doesnt answer"
                        });
                    }
                }

            }


        }


        public override Task<AuthenticateTokenResponse> AuthenticateToken(AuthenticateTokenRequest request, ServerCallContext context)
        {
            System.Console.WriteLine("Вызов метода AuthenticateToken спараметром: " + request.Token);

            try
            {
                var response = clientTM.AuthenticateToken(new TradeBot.TradeMarket.TradeMarketService.v1.AuthenticateTokenRequest { Token = request.Token });

                System.Console.WriteLine("Возврат значения из AuthenticateToken:" + response.Response.ToString());
                return Task.FromResult(new AuthenticateTokenResponse
                {
                    //Response = new TradeBot.Common.v1.DefaultResponse { Code=TradeBot.Common.v1.ReplyCode.Succeed,Message="otvet"}
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
            using var response = clientTM.Slots(new TradeBot.TradeMarket.TradeMarketService.v1.SlotsRequest { Empty = request.Empty });

            while (await response.ResponseStream.MoveNext())
            {
                await responseStream.WriteAsync(new SlotsResponse
                {
                    SlotName = response.ResponseStream.Current.SlotName
                });
            }
        }


        public override Task<SwitchBotResponse> SwitchBot(SwitchBotRequest request, ServerCallContext context)
        {
            System.Console.WriteLine("Вызов метода StartBot с параметром: " + request.Config.ToString());
            try
            {

                var response = clientRelay.StartBot(new TradeBot.Relay.RelayService.v1.StartBotRequest { Config = request.Config });

                System.Console.WriteLine("Возврат значения из StartBot: " + response.Response.ToString());

                return Task.FromResult(new SwitchBotResponse
                {
                    Response = response.Response
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка работы метода StarBot");
                Console.WriteLine("Exception: " + e.Message);
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
            var response = clientRelay.SubscribeLogs(new TradeBot.Relay.RelayService.v1.SubscribeLogsRequest { Request = request.R }, cancellationToken: context.CancellationToken);

            while (await response.ResponseStream.MoveNext())
            {
                //токен на эксепшен
                context.CancellationToken.ThrowIfCancellationRequested();
                await responseStream.WriteAsync(new SubscribeLogsResponse
                {
                    Response = response.ResponseStream.Current.Response
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
