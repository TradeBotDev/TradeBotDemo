using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auth = TradeBot.Account.AccountService.v1.Account.AccountClient;
using Exch = TradeBot.Account.AccountService.v1.ExchangeAccess.ExchangeAccessClient;
using Ref = TradeBot.Facade.FacadeService.v1;

namespace Facade
{
    public class AutorisationClass
    {
        private GrpcChannel _channel => GrpcChannel.ForAddress("https://localhost:5000");
        private Auth _client => new TradeBot.Account.AccountService.v1.Account.AccountClient(_channel);

        private TradeBot.Account.AccountService.v1.Account.AccountClient clientAccount = new TradeBot.Account.AccountService.v1.Account.AccountClient(GrpcChannel.ForAddress("https://localhost:5000"));


        public Task<Ref.LoginReply> LoginAuth(Ref.LoginRequest request, ServerCallContext context, Auth auth)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;

                    var response = _client.Login(new TradeBot.Account.AccountService.v1.LoginRequest
                    {
                        Email = request.Email,
                        SaveExchangesAfterLogout = request.SaveExchangesAfterLogout,
                        Password = request.Password
                    });
                    Log.Information($"Function: Login \n args: request={request}");
                    Log.Information($"Function: Login \n args: response={response}");
                    return Task.FromResult(new Ref.LoginReply
                    {
                        Message = response.Message,
                        SessionId = response.SessionId,
                        Result = (Ref.ActionCode)response.Result
                    });
                }
                catch (RpcException e)
                {
                    Log.Error("Exception:" + e.Message);
                }
            }
            Log.Information("Client disconnected");
            return Task.FromResult(new Ref.LoginReply { });
        }

        public Task<Ref.LogoutReply> LogoutAuth(Ref.SessionRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;

                    var response = _client.Logout(new TradeBot.Account.AccountService.v1.SessionRequest { SessionId = request.SessionId });
                    Log.Information($"Function: Logout \n args: request={request}");
                    Log.Information($"Function: Logout \n args: response={response}");

                    return Task.FromResult(new Ref.LogoutReply
                    {
                        Message = response.Message,
                        Result = (Ref.ActionCode)response.Result
                    });

                }
                catch (RpcException e)
                {
                    Log.Error("Exception:" + e.Message);
                }
            }
            Log.Information("Client disconnected");
            return Task.FromResult(new Ref.LogoutReply { });
        }

        public Task<Ref.RegisterReply> RegisterAuth(Ref.RegisterRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;

                    var response = _client.Register(new TradeBot.Account.AccountService.v1.RegisterRequest
                    {
                        Email = request.Email,
                        Password = request.Password,
                        VerifyPassword = request.VerifyPassword
                    });
                    Log.Information($"Function: Register \n args: request={request}");
                    Log.Information($"Function: Register \n args: response={response}");
                    return Task.FromResult(new Ref.RegisterReply
                    {
                        Message = response.Message,
                        Result = (Ref.ActionCode)response.Result

                    });
                }
                catch (RpcException e)
                {
                    Log.Error("Exception:" + e.Message);
                }
            }
            Log.Information("Client disconnected");
            return Task.FromResult(new Ref.RegisterReply { });
        }

        public Task<Ref.SessionReply> IsValidSessionAuth(Ref.SessionRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;

                    var response = _client.IsValidSession(new TradeBot.Account.AccountService.v1.SessionRequest { SessionId = request.SessionId });
                    Log.Information($"Function: IsValidSession \n args: request={request}");
                    Log.Information($"Function: IsValidSession \n args: response={response}");
                    return Task.FromResult(new Ref.SessionReply
                    {
                        IsValid = response.IsValid,
                        Message = response.Message
                    });
                }
                catch (RpcException e)
                {
                    Log.Error("Exception:" + e.Message);
                }
            }
            Log.Information("Client disconnected");
            return Task.FromResult(new Ref.SessionReply { });
        }

        public Task<Ref.CurrentAccountReply> CurrentAccountDataAuth(Ref.SessionRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;

                    var response = _client.CurrentAccountData(new TradeBot.Account.AccountService.v1.SessionRequest { SessionId = request.SessionId });
                    Log.Information($"Function: CurrentAccountData \n args: request={request}");
                    var accountResponse = new Ref.CurrentAccountReply()
                    {
                        Message = response.Message,
                        Result = (Ref.ActionCode)response.Result,
                        CurrentAccount = new Ref.AccountInfo
                        {
                            AccountId = response.CurrentAccount.AccountId,
                            Email = response.CurrentAccount.Email
                        }
                    };
                    foreach (var item in response.CurrentAccount.Exchanges)
                    {
                        accountResponse.CurrentAccount.Exchanges.Add(new Ref.ExchangeAccessInfo
                        {
                            Secret = item.Secret,
                            Code = (Ref.ExchangeCode)item.Code,
                            ExchangeAccessId = item.ExchangeAccessId,
                            Name = item.Name,
                            Token = item.Token
                        });
                    }
                    Log.Information($"Function: CurrentAccountData \n args: response={response}");
                    return Task.FromResult(accountResponse);
                }
                catch (RpcException e)
                {
                    Log.Error("Exception:" + e.Message);
                }
            }
            Log.Information("Client disconnected");
            return Task.FromResult(new Ref.CurrentAccountReply { });
        }

        public Task<Ref.AddExchangeAccessReply> AddExchangeAccessAuth(Ref.AddExchangeAccessRequest request, ServerCallContext context)
        {
            //while (true)
            //{
            //    try
            //    {
            //        if (context.CancellationToken.IsCancellationRequested) break;

            //        var response = Client.AddExchangeAccess(new TradeBot.Account.AccountService.v1.AddExchangeAccessRequest
            //        {
            //            Code = (TradeBot.Account.AccountService.v1.ExchangeCode)request.Code,
            //            Secret = request.Secret,
            //            ExchangeName = request.ExchangeName,
            //            SessionId = request.SessionId,
            //            Token = request.Token
            //        });
            //        Log.Information($"Function: AddExchangeAccess \n args: request={request}");
            //        Log.Information($"Function: AddExchangeAccess \n args: response={response}");
            //        return Task.FromResult(new Ref.AddExchangeAccessReply
            //        {
            //            Message = response.Message,
            //            Result = (Ref.ActionCode)response.Result
            //        });
            //    }
            //    catch (RpcException e)
            //    {
            //        Log.Error("Exception:" + e.Message);
            //    }
            //}
            Log.Information("Remote function call...");
            return Task.FromResult(new Ref.AddExchangeAccessReply { Message = "Remote function call..." });
        }

        public Task<Ref.AllExchangesBySessionReply> AllExchangesBySessionAuth(Ref.SessionRequest request, ServerCallContext context)
        {
            //while (true)
            //{
            //    try
            //    {
            //        if (context.CancellationToken.IsCancellationRequested) break;
            //        var response = Client.AllExchangesBySession(new TradeBot.Account.AccountService.v1.SessionRequest { SessionId = request.SessionId });
            //        Log.Information($"Function: AllExchangesBySession \n args: request={request}");
            //        var accountResponse = new AllExchangesBySessionReply()
            //        {
            //            Message = response.Message,
            //            Result = (Ref.ActionCode)response.Result
            //        };
            //        foreach (var item in response.Exchanges)
            //        {
            //            accountResponse.Exchanges.Add(new Ref.ExchangeAccessInfo
            //            {
            //                Secret = item.Secret,
            //                Code = (Ref.ExchangeCode)item.Code,
            //                ExchangeAccessId = item.ExchangeAccessId,
            //                Name = item.Name,
            //                Token = item.Token
            //            });
            //        }
            //        Log.Information($"Function: AllExchangesBySession \n args: response={response}");
            //        return Task.FromResult(accountResponse);
            //    }
            //    catch (RpcException e)
            //    {
            //        Log.Error("Exception:" + e.Message);
            //    }
            //}
            Log.Information("Client disconnected");
            return Task.FromResult(new Ref.AllExchangesBySessionReply { Message = "Remote function call..." });
        }

        public Task<Ref.DeleteExchangeAccessReply> DeleteExchangeAccessAuth(Ref.DeleteExchangeAccessRequest request, ServerCallContext context)
        {
            //while (true)
            //{
            //    try
            //    {
            //        if (context.CancellationToken.IsCancellationRequested) break;

            //        var response = Client.DeleteExchangeAccess(new TradeBot.Account.AccountService.v1.DeleteExchangeAccessRequest { SessionId = request.SessionId, Code = (TradeBot.Account.AccountService.v1.ExchangeCode)request.Code });
            //        Log.Information($"Function: DeleteExchangeAccess \n args: request={request}");
            //        Log.Information($"Function: DeleteExchangeAccess \n args: response={response}");
            //        return Task.FromResult(new Ref.DeleteExchangeAccessReply
            //        {
            //            Message = response.Message,
            //            Result = (Ref.ActionCode)response.Result
            //        });
            //    }
            //    catch (RpcException e)
            //    {
            //        Log.Error("Exception:" + e);
            //    }
            //}
            return Task.FromResult(new Ref.DeleteExchangeAccessReply { Message = "Remote function call..." });
        }

        public Task<Ref.ExchangeBySessionReply> ExchangeBySessionAuth(Ref.ExchangeBySessionRequest request, ServerCallContext context)
        {
            //while (true)
            //{
            //    try
            //    {
            //        if (context.CancellationToken.IsCancellationRequested) break;

            //        var response = Client.ExchangeBySession(new TradeBot.Account.AccountService.v1.ExchangeBySessionRequest { SessionId = request.SessionId, Code = (TradeBot.Account.AccountService.v1.ExchangeCode)request.Code });
            //        Log.Information($"Function: ExchangeBySession \n args: request={request}");
            //        Log.Information($"Function: ExchangeBySession \n args: response={response}");
            //        return Task.FromResult(new Ref.ExchangeBySessionReply
            //        {
            //            Message = response.Message,
            //            Result = (Ref.ActionCode)response.Result,
            //            Exchange = new Ref.ExchangeAccessInfo
            //            {
            //                Secret = response.Exchange.Secret,
            //                Code = (Ref.ExchangeCode)response.Exchange.Code,
            //                ExchangeAccessId = response.Exchange.ExchangeAccessId,
            //                Name = response.Exchange.Name,
            //                Token = response.Exchange.Token
            //            }
            //        });
            //    }
            //    catch (RpcException e)
            //    {
            //        Log.Error("Exception:" + e.Message);
            //    }
            //}
            Log.Information("Remote function call...");
            return Task.FromResult(new Ref.ExchangeBySessionReply { Message = "Remote function call..." });

        }
    }
}
