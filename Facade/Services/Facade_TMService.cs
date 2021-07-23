using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Threading.Tasks;
using TradeBot.Facade.FacadeService.v1;

namespace Facade
{
    public class FacadeTMService : TradeBot.Facade.FacadeService.v1.FacadeService.FacadeServiceBase
    {
        //TODO вынести
        private TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient clientTM = new TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient(GrpcChannel.ForAddress("https://localhost:5005"));
        private TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient clientRelay = new TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient(GrpcChannel.ForAddress("https://localhost:5004"));
        private TradeBot.Account.AccountService.v1.Account.AccountClient clientAccount = new TradeBot.Account.AccountService.v1.Account.AccountClient(GrpcChannel.ForAddress("https://localhost:5000"));
        private TradeBot.Account.AccountService.v1.ExchangeAccess.ExchangeAccessClient clientExchange = new TradeBot.Account.AccountService.v1.ExchangeAccess.ExchangeAccessClient(GrpcChannel.ForAddress("https://localhost:5000"));
        private readonly ILogger<FacadeTMService> _logger;
        public FacadeTMService(ILogger<FacadeTMService> logger)
        {
            _logger = logger;
        }
        #region TradeMarket
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
                                    Money = response.ResponseStream.Current.Response.Balance
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

        public override async Task Slots(SlotsRequest request, IServerStreamWriter<SlotsResponse> responseStream, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    var response = clientTM.Slots(new TradeBot.TradeMarket.TradeMarketService.v1.SlotsRequest { });
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

        public override async Task SubscribeLogsTM(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    var response = clientTM.SubscribeLogs(new TradeBot.TradeMarket.TradeMarketService.v1.SubscribeLogsRequest { Request = request.R });
                    if (!context.CancellationToken.IsCancellationRequested)
                    {
                        if (response.ResponseStream.Current != null)
                        {
                            while (await response.ResponseStream.MoveNext())
                            {
                                await responseStream.WriteAsync(new SubscribeLogsResponse
                                {
                                    Response = response.ResponseStream.Current.Response
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
        #endregion
        #region Relay
        public override async Task SubscribeLogsRelay(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    var response = clientRelay.SubscribeLogs(new TradeBot.Relay.RelayService.v1.SubscribeLogsRequest { Request = request.R });
                    if (!context.CancellationToken.IsCancellationRequested)
                    {
                        if (response.ResponseStream.Current != null)
                        {
                            while (await response.ResponseStream.MoveNext())
                            {
                                await responseStream.WriteAsync(new SubscribeLogsResponse
                                {
                                    Response = response.ResponseStream.Current.Response
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

                    var response = clientRelay.StartBot(new TradeBot.Relay.RelayService.v1.StartBotRequest 
                    { 
                        Config = request.Config 
                    }, context.RequestHeaders);
                    
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
                    Log.Information("Exception:" + e);
                }
            }
            return Task.FromResult(new UpdateServerConfigResponse { });
        }
        #endregion
        #region Account
        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;

                    var response = clientAccount.Login(new TradeBot.Account.AccountService.v1.LoginRequest
                    {
                        Email = request.Email,
                        SaveExchangesAfterLogout = request.SaveExchangesAfterLogout,
                        Password = request.Password
                    });
                    return Task.FromResult(new LoginReply 
                    { 
                        Message=response.Message,
                        SessionId=response.SessionId,
                        Result= (ActionCode)response.Result
                    });
                }
                catch (RpcException e)
                {
                    Log.Information("Exception:" + e);
                }
            }
            return Task.FromResult(new LoginReply { });
        }

        public override Task<LogoutReply> Logout(SessionRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;

                    var response = clientAccount.Logout(new TradeBot.Account.AccountService.v1.SessionRequest { SessionId = request.SessionId });
                    return Task.FromResult(new LogoutReply 
                    { 
                        Message=response.Message,
                        Result= (ActionCode)response.Result
                    });
                    
                }
                catch (RpcException e)
                {
                    Log.Information("Exception:" + e);
                }
            }
            return Task.FromResult(new LogoutReply { });
        }

        public override Task<RegisterReply> Register(RegisterRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;

                    var response = clientAccount.Register(new TradeBot.Account.AccountService.v1.RegisterRequest 
                    { 
                        Email= request.Email,
                        Firstname=request.Firstname,
                        Lastname=request.Lastname,
                        Password=request.Password,
                        PhoneNumber=request.PhoneNumber,
                        VerifyPassword=request.VerifyPassword
                    });
                    return Task.FromResult(new RegisterReply 
                    {
                        Message=response.Message,
                        Result= (ActionCode)response.Result

                    });
                }
                catch (RpcException e)
                {
                    Log.Information("Exception:" + e);
                }
            }
            return Task.FromResult(new RegisterReply { });
        }

        public override Task<SessionReply> IsValidSession(SessionRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;

                    var response = clientAccount.IsValidSession(new TradeBot.Account.AccountService.v1.SessionRequest { SessionId = request.SessionId });
                    return Task.FromResult(new SessionReply
                    {
                        IsValid=response.IsValid,
                        Message=response.Message
                    });
                }
                catch (RpcException e)
                {
                    Log.Information("Exception:" + e);
                }
            }
            return Task.FromResult(new SessionReply { });
        }

        public override Task<CurrentAccountReply> CurrentAccountData(SessionRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;

                    var response = clientAccount.CurrentAccountData(new TradeBot.Account.AccountService.v1.SessionRequest { SessionId = request.SessionId });
     
                    var accountResponse = new CurrentAccountReply()
                    {
                        Message = response.Message,
                        Result = (ActionCode)response.Result,
                        CurrentAccount = new AccountInfo
                        {
                            AccountId = response.CurrentAccount.AccountId,
                            Email = response.CurrentAccount.Email,
                            Firstname = response.CurrentAccount.Firstname,
                            Lastname = response.CurrentAccount.Lastname,
                            PhoneNumber = response.CurrentAccount.PhoneNumber,
                        }
                    };
                    foreach (var item in response.CurrentAccount.Exchanges)
                    {
                        accountResponse.CurrentAccount.Exchanges.Add(new ExchangeAccessInfo
                        {
                            Secret = item.Secret,
                            Code= (ExchangeCode)item.Code,
                            ExchangeAccessId=item.ExchangeAccessId,
                            Name=item.Name,
                            Token=item.Token
                        });
                    }
                    return Task.FromResult(accountResponse);
                }
                catch (RpcException e)
                {
                    Log.Information("Exception:" + e);
                }
            }
            return Task.FromResult(new CurrentAccountReply { });
        }

        public override Task<AddExchangeAccessReply> AddExchangeAccess(AddExchangeAccessRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;

                    var response = clientExchange.AddExchangeAccess(new TradeBot.Account.AccountService.v1.AddExchangeAccessRequest 
                    { 
                        Code= (TradeBot.Account.AccountService.v1.ExchangeCode)request.Code,
                        Secret =request.Secret,
                        ExchangeName=request.ExchangeName,
                        SessionId=request.SessionId,
                        Token=request.Token
                    });
                    return Task.FromResult(new AddExchangeAccessReply
                    {
                        Message=response.Message,
                        Result= (ActionCode)response.Result
                    });
                }
                catch (RpcException e)
                {
                    Log.Information("Exception:" + e);
                }
            }
            return Task.FromResult(new AddExchangeAccessReply { });
        }

        public override Task<AllExchangesBySessionReply> AllExchangesBySession(SessionRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;
                    var response = clientExchange.AllExchangesBySession(new TradeBot.Account.AccountService.v1.SessionRequest{SessionId=request.SessionId});
                    var accountResponse = new AllExchangesBySessionReply()
                    {
                        Message = response.Message,
                        Result = (ActionCode)response.Result
                    };
                    foreach (var item in response.Exchanges)
                    {
                        accountResponse.Exchanges.Add(new ExchangeAccessInfo
                        {
                            Secret = item.Secret,
                            Code = (ExchangeCode)item.Code,
                            ExchangeAccessId = item.ExchangeAccessId,
                            Name = item.Name,
                            Token = item.Token
                        });
                    }

                    return Task.FromResult(accountResponse);
                }
                catch (RpcException e)
                {
                    Log.Information("Exception:" + e);
                }
            }
            return Task.FromResult(new AllExchangesBySessionReply { });
        }

        public override Task<DeleteExchangeAccessReply> DeleteExchangeAccess(DeleteExchangeAccessRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;

                    var response = clientExchange.DeleteExchangeAccess(new TradeBot.Account.AccountService.v1.DeleteExchangeAccessRequest { SessionId=request.SessionId,Code= (TradeBot.Account.AccountService.v1.ExchangeCode)request.Code});
                    return Task.FromResult(new DeleteExchangeAccessReply
                    {
                        Message=response.Message,
                        Result= (ActionCode)response.Result
                    });
                }
                catch (RpcException e)
                {
                    Log.Information("Exception:" + e);
                }
            }
            return Task.FromResult(new DeleteExchangeAccessReply { });
        }
        public override Task<ExchangeBySessionReply> ExchangeBySession(ExchangeBySessionRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;

                    var response = clientExchange.ExchangeBySession(new TradeBot.Account.AccountService.v1.ExchangeBySessionRequest { SessionId=request.SessionId,Code= (TradeBot.Account.AccountService.v1.ExchangeCode)request.Code});
                    return Task.FromResult(new ExchangeBySessionReply
                    {
                        Message=response.Message,
                        Result= (ActionCode)response.Result,
                        Exchange = new ExchangeAccessInfo 
                        { 
                            Secret=response.Exchange.Secret,
                            Code= (ExchangeCode)response.Exchange.Code,
                            ExchangeAccessId=response.Exchange.ExchangeAccessId,
                            Name=response.Exchange.Name,
                            Token=response.Exchange.Token
                        }
                    });
                }
                catch (RpcException e)
                {
                    Log.Information("Exception:" + e);
                }
            }
            return Task.FromResult(new ExchangeBySessionReply { });
        }
        #endregion

        
    }

}
