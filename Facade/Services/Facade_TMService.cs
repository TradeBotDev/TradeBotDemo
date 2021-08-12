using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
using System.Threading.Tasks;
using TradeBot.Facade.FacadeService.v1;

namespace Facade
{
    public class FacadeTMService : TradeBot.Facade.FacadeService.v1.FacadeService.FacadeServiceBase
    {
        //TODO вынести
        private TradeBot.Account.AccountService.v1.Account.AccountClient clientAccount = new TradeBot.Account.AccountService.v1.Account.AccountClient(GrpcChannel.ForAddress("https://localhost:5000"));
        private TradeBot.Account.AccountService.v1.ExchangeAccess.ExchangeAccessClient clientExchange = new TradeBot.Account.AccountService.v1.ExchangeAccess.ExchangeAccessClient(GrpcChannel.ForAddress("https://localhost:5000"));
        private TradeBot.History.HistoryService.v1.HistoryService.HistoryServiceClient clientHistory = new TradeBot.History.HistoryService.v1.HistoryService.HistoryServiceClient(GrpcChannel.ForAddress("https://localhost:5007"));
        private TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient clientRelay = new TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient(GrpcChannel.ForAddress("https://localhost:5004"));
        private TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient clientTM = new TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient(GrpcChannel.ForAddress("http://localhost:5005"));
        private IAsyncStreamReader<TradeBot.Relay.RelayService.v1.SubscribeLogsResponse> stream;
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
                            System.Diagnostics.StackFrame frame = new System.Diagnostics.StackFrame();
                            var method = frame.GetMethod();
                            Log.Information("{@Where}: {@MethodName} \n args: request={@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request);
                            while (await response.ResponseStream.MoveNext())
                            {
                                Log.Information("{@Where}: {@MethodName} \n args: response={@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response.ResponseStream.Current.Response.Balance);

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
                        Log.Information("{@Where}: Client disconnected", "Facade");
                        break;
                    }
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception" + e.Message, "Facade");
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
                            Log.Information("{@Where}: {@MethodName} \n args: request={@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request); while (await response.ResponseStream.MoveNext())
                            {
                                Log.Information("{@Where}: {@MethodName} \n args: response={@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response.ResponseStream.Current.SlotName);

                                await responseStream.WriteAsync(new SlotsResponse
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
                        Log.Information("{@Where}: Client disconnected", "Facade");
                        break;
                    }
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception" + e.Message, "Facade");
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
                            Log.Information("{@Where}: {@MethodName} \n args: request={@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request); while (await response.ResponseStream.MoveNext())
                            {
                                Log.Information("{@Where}: {@MethodName} \n args: response={@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response.ResponseStream.Current.Response); await responseStream.WriteAsync(new SubscribeLogsResponse
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
                        Log.Information("{@Where}: Client disconnected", "Facade");
                        break;
                    }
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception" + e.Message, "Facade");
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
                    Log.Information("{@Where}: {@MethodName} \n args: request={@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request);
                    Log.Information("{@Where}: {@MethodName} \n args: response={@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response);
                    return Task.FromResult(new AuthenticateTokenResponse { Response = response.Response });
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception" + e.Message, "Facade");
                }
            }
            return Task.FromResult(new AuthenticateTokenResponse { });

        }



        #endregion

        #region Relay
        public override async Task SubscribeLogsRelay(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            try
            {
                stream = clientRelay.SubscribeLogs(new TradeBot.Relay.RelayService.v1.SubscribeLogsRequest { Request = request.R }, context.RequestHeaders).ResponseStream;
                Log.Information("{@Where}: SubscribeLogsRelay \n args: request={@request}", "Facade", request);
                while (!context.CancellationToken.IsCancellationRequested)
                {

                    while (await stream.MoveNext())
                    {
                        Log.Information("{@Where}: SubscribeLogsRelay \n args: response={@response}", "Facade", stream.Current.Response);
                        await responseStream.WriteAsync(new SubscribeLogsResponse
                        {
                            Response = stream.Current.Response
                        });
                    }
                }
                Log.Information("{@Where}: Client disconnected", "Facade");
            }
            catch (RpcException e)
            {
                Log.Error("{@Where}: Exception" + e.Message, "Facade");
            }
        }

        public override Task<DeleteOrderResponse> DeleteOrder(DeleteOrderRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;
                    var response = clientRelay.DeleteOrder(new TradeBot.Relay.RelayService.v1.DeleteOrderRequest { }, context.RequestHeaders);
                    Log.Information("{@Where}: {@MethodName} \n args: request={@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request);
                    Log.Information("{@Where}: {@MethodName} \n args: response={@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response);
                    return Task.FromResult(new DeleteOrderResponse { Response = new TradeBot.Common.v1.DefaultResponse
                    {
                        Code = response.Response.Code,
                        Message = response.Response.Message
                    }
                    });
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception" + e.Message, "Facade");
                }
            }
            Log.Information("{@Where}: Client disconnected", "Facade");
            return Task.FromResult(new DeleteOrderResponse { });
        }
        public override Task<SwitchBotResponse> StartBot(SwitchBotRequest request, ServerCallContext context)
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
                    Log.Information("{@Where}: {@MethodName} \n args: request={@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request);
                    Log.Information("{@Where}: {@MethodName} \n args: response={@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response);
                    return Task.FromResult(new SwitchBotResponse
                    {
                        Response = response.Response
                    });
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception" + e.Message, "Facade");
                }
            }
            Log.Information("{@Where}: Client disconnected", "Facade");
            return Task.FromResult(new SwitchBotResponse { });
        }
        public override Task<StopBotResponse> StopBot(StopBotRequest request, ServerCallContext context)
        {
            {
                while (true)
                {
                    try
                    {
                        if (context.CancellationToken.IsCancellationRequested) break;
                        var response = clientRelay.StopBot(new TradeBot.Relay.RelayService.v1.StopBotRequest
                        {
                            Request = new TradeBot.Common.v1.UpdateServerConfigRequest
                            {
                                Config = request.Request.Config,
                                Switch = request.Request.Switch
                            }
                        }, context.RequestHeaders);
                        Log.Information("{@Where}: {@MethodName} \n args: request={@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request);
                        Log.Information("{@Where}: {@MethodName} \n args: response={@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response);
                        return Task.FromResult(new StopBotResponse { });
                    }
                    catch (RpcException e)
                    {
                        Log.Error("{@Where}: Exception" + e.Message, "Facade");
                    }
                }
                Log.Information("{@Where}: Client disconnected", "Facade");
                return Task.FromResult(new StopBotResponse { });
            }
        }
        public async override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {

                    if (context.CancellationToken.IsCancellationRequested) break;
                    var response = await clientRelay.UpdateServerConfigAsync(new TradeBot.Relay.RelayService.v1.UpdateServerConfigRequest() { Request = request.Request }, context.RequestHeaders);
                    Log.Information("{@Where}: {@MethodName} \n args: request={@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request);
                    Log.Information("{@Where}: {@MethodName} \n args: response: {@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response);
                    return new UpdateServerConfigResponse();
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception:" + e.Message, "Facade");
                }
            }
            Log.Information("{@Where}: Client disconnected", "Facade");
            return new UpdateServerConfigResponse();
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
                    Log.Information("{@Where}: {@MethodName} \n args: request: {@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request);
                    Log.Information("{@Where}: {@MethodName} \n args: response: {@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response);
                    return Task.FromResult(new LoginReply
                    {
                        Message = response.Message,
                        SessionId = response.SessionId,
                        Result = (ActionCode)response.Result
                    });
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception:" + e.Message, "Facade");
                }
            }
            Log.Information("{@Where}: Client disconnected", "Facade");
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
                    Log.Information("{@Where}: {@MethodName} \n args: request: {@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request);
                    Log.Information("{@Where}: {@MethodName} \n args: response: {@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response);

                    return Task.FromResult(new LogoutReply
                    {
                        Message = response.Message,
                        Result = (ActionCode)response.Result
                    });

                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception:" + e.Message, "Facade");
                }
            }
            Log.Information("{@Where}: Client disconnected", "Facade");
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
                        Email = request.Email,
                        Password = request.Password,
                        VerifyPassword = request.VerifyPassword
                    });
                    Log.Information("{@Where}: {@MethodName} \n args: request: {@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request);
                    Log.Information("{@Where}: {@MethodName} \n args: response: {@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response);
                    return Task.FromResult(new RegisterReply
                    {
                        Message = response.Message,
                        Result = (ActionCode)response.Result

                    });
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception:" + e.Message, "Facade");
                }
            }
            Log.Information("{@Where}: Client disconnected", "Facade");
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
                    Log.Information("{@Where}: {@MethodName} \n args: request: {@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request);
                    Log.Information("{@Where}: {@MethodName} \n args: response: {@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response);
                    return Task.FromResult(new SessionReply
                    {
                        IsValid = response.IsValid,
                        Message = response.Message
                    });
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception:" + e.Message, "Facade");
                }
            }
            Log.Information("{@Where}: Client disconnected", "Facade");
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
                    Log.Information("{@Where}: {@MethodName} \n args: request: {@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request);
                    var accountResponse = new CurrentAccountReply()
                    {
                        Message = response.Message,
                        Result = (ActionCode)response.Result,
                        CurrentAccount = new AccountInfo
                        {
                            AccountId = response.CurrentAccount.AccountId,
                            Email = response.CurrentAccount.Email
                        }
                    };
                    foreach (var item in response.CurrentAccount.Exchanges)
                    {
                        accountResponse.CurrentAccount.Exchanges.Add(new ExchangeAccessInfo
                        {
                            Secret = item.Secret,
                            Code = (ExchangeCode)item.Code,
                            ExchangeAccessId = item.ExchangeAccessId,
                            Name = item.Name,
                            Token = item.Token
                        });
                    }
                    Log.Information("{@Where}: {@MethodName} \n args: response: {@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response);
                    return Task.FromResult(accountResponse);
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception:" + e.Message, "Facade");
                }
            }
            Log.Information("{@Where}: Client disconnected", "Facade");
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
                        Code = (TradeBot.Account.AccountService.v1.ExchangeCode)request.Code,
                        Secret = request.Secret,
                        ExchangeName = request.ExchangeName,
                        SessionId = request.SessionId,
                        Token = request.Token
                    });
                    Log.Information("{@Where}: {@MethodName} \n args: request: {@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request);
                    Log.Information("{@Where}: {@MethodName} \n args: response: {@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response);
                    return Task.FromResult(new AddExchangeAccessReply
                    {
                        Message = response.Message,
                        Result = (ActionCode)response.Result
                    });
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception:" + e.Message, "Facade");
                }
            }
            Log.Information("{@Where}: Client disconnected", "Facade");
            return Task.FromResult(new AddExchangeAccessReply { });
        }

        public override Task<AllExchangesBySessionReply> AllExchangesBySession(SessionRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;
                    var response = clientExchange.AllExchangesBySession(new TradeBot.Account.AccountService.v1.SessionRequest { SessionId = request.SessionId });
                    Log.Information("{@Where}: {@MethodName} \n args: request: {@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request);
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
                    Log.Information("{@Where}: {@MethodName} \n args: response: {@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response);
                    return Task.FromResult(accountResponse);
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception:" + e.Message, "Facade");
                }
            }
            Log.Information("{@Where}: Client disconnected", "Facade");
            return Task.FromResult(new AllExchangesBySessionReply { });
        }

        public override Task<DeleteExchangeAccessReply> DeleteExchangeAccess(DeleteExchangeAccessRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;

                    var response = clientExchange.DeleteExchangeAccess(new TradeBot.Account.AccountService.v1.DeleteExchangeAccessRequest { SessionId = request.SessionId, Code = (TradeBot.Account.AccountService.v1.ExchangeCode)request.Code });
                    Log.Information("{@Where}: {@MethodName} \n args: request: {@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request);
                    Log.Information("{@Where}: {@MethodName} \n args: response: {@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response);
                    return Task.FromResult(new DeleteExchangeAccessReply
                    {
                        Message = response.Message,
                        Result = (ActionCode)response.Result
                    });
                }
                catch (RpcException e)
                {
                    Log.Error("Exception:" + e);
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

                    var response = clientExchange.ExchangeBySession(new TradeBot.Account.AccountService.v1.ExchangeBySessionRequest { SessionId = request.SessionId, Code = (TradeBot.Account.AccountService.v1.ExchangeCode)request.Code });
                    Log.Information("{@Where}: {@MethodName} \n args: request: {@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request);
                    Log.Information("{@Where}: {@MethodName} \n args: response: {@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response);
                    return Task.FromResult(new ExchangeBySessionReply
                    {
                        Message = response.Message,
                        Result = (ActionCode)response.Result,
                        Exchange = new ExchangeAccessInfo
                        {
                            Secret = response.Exchange.Secret,
                            Code = (ExchangeCode)response.Exchange.Code,
                            ExchangeAccessId = response.Exchange.ExchangeAccessId,
                            Name = response.Exchange.Name,
                            Token = response.Exchange.Token
                        }
                    });
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception:" + e.Message, "Facade");
                }
            }
            Log.Information("{@Where}: Client disconnected", "Facade");
            return Task.FromResult(new ExchangeBySessionReply { });
        }

        #endregion

        #region History
        public async override Task SubscribeEvents(SubscribeEventsRequest request, IServerStreamWriter<SubscribeEventsResponse> responseStream, ServerCallContext context)
        {
            try
            {
                var response = clientHistory.SubscribeEvents(new TradeBot.History.HistoryService.v1.SubscribeEventsRequest { Sessionid = request.Sessionid }, context.RequestHeaders);

                while (await response.ResponseStream.MoveNext())
                {
                    switch (response.ResponseStream.Current.EventTypeCase)
                    {
                        case TradeBot.History.HistoryService.v1.SubscribeEventsResponse.EventTypeOneofCase.Balance:
                            await responseStream.WriteAsync(new SubscribeEventsResponse
                            {
                                Balance = new PublishBalanceEvent
                                {
                                    Sessionid = response.ResponseStream.Current.Balance.Sessionid,
                                    Balance = response.ResponseStream.Current.Balance.Balance,
                                    Time = response.ResponseStream.Current.Balance.Time
                                }
                            });
                            break;
                        case TradeBot.History.HistoryService.v1.SubscribeEventsResponse.EventTypeOneofCase.Order:
                            await responseStream.WriteAsync(new SubscribeEventsResponse
                            {
                                Order = new PublishOrderEvent
                                {
                                    ChangesType = response.ResponseStream.Current.Order.ChangesType,
                                    Message = response.ResponseStream.Current.Order.Message,
                                    Sessionid = response.ResponseStream.Current.Order.Sessionid,
                                    Order = response.ResponseStream.Current.Order.Order,
                                    Time = response.ResponseStream.Current.Order.Time
                                }
                            });
                            break;
                        case TradeBot.History.HistoryService.v1.SubscribeEventsResponse.EventTypeOneofCase.None:
                            Log.Information("{@Where}: {@MethodName} Get none", "Facade", nameof(SubscribeEvents));
                            break;
                    }
                }
            }
            catch (System.Exception e)
            {
                Log.Error("{@Where}: {@MethodName}-Exception: {@Exception}","Facade",nameof(SubscribeEvents),e.Message);
            }
        }
    }
    #endregion
}


