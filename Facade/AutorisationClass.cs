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
        private GrpcChannel _channel => GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("ACCOUNT_CONNECTION_STRING"));
        private GrpcChannel Channel { get => _channel; }

        private Auth _clientAccount => new Auth(Channel);
        public Auth ClientAccount

        {
            get => _clientAccount;
        }

        private Exch _clientExc => new Exch(Channel);
        public Exch ClientExc

        {
            get => _clientExc;
        }
        public async Task<Ref.LoginReply> Account_Login(Ref.LoginRequest request, string methodName)
        {
            TradeBot.Account.AccountService.v1.LoginReply response = null;
            async Task<TradeBot.Account.AccountService.v1.LoginReply> task()
            {
                response =await ClientAccount.LoginAsync(new TradeBot.Account.AccountService.v1.LoginRequest
                {
                    Email = request.Email,
                    SaveExchangesAfterLogout = request.SaveExchangesAfterLogout,
                    Password = request.Password
                });
                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);
            return await Generalization.ReturnResponse(new Ref.LoginReply { SessionId = response.SessionId, Message = response.Message, Result = (Ref.ActionCode)response.Result }, methodName);
        }
        public async Task<Ref.LogoutReply> Account_Logout(Ref.SessionRequest request, ServerCallContext context, string methodName)
        {
            TradeBot.Account.AccountService.v1.LogoutReply response = null;
            async Task<TradeBot.Account.AccountService.v1.LogoutReply> task()
            {
                response = await ClientAccount.LogoutAsync(new TradeBot.Account.AccountService.v1.SessionRequest
                {
                    SessionId = request.SessionId
                }, context.RequestHeaders);
                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);
            return await Generalization.ReturnResponse(new TradeBot.Facade.FacadeService.v1.LogoutReply { Message = response.Message, Result = (Ref.ActionCode)response.Result }, methodName);
        }
        public async Task<Ref.RegisterReply> Account_Register(Ref.RegisterRequest request, ServerCallContext context, string methodName)
        {
            TradeBot.Account.AccountService.v1.RegisterReply response = null;
            async Task<TradeBot.Account.AccountService.v1.RegisterReply> task()
            {
                response = await ClientAccount.RegisterAsync(new TradeBot.Account.AccountService.v1.RegisterRequest
                {
                    Email = request.Email,
                    Password = request.Password,
                    VerifyPassword = request.VerifyPassword
                }, context.RequestHeaders);
                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);
            return await Generalization.ReturnResponse(new TradeBot.Facade.FacadeService.v1.RegisterReply { Message = response.Message, Result = (Ref.ActionCode)response.Result }, methodName);
        }
        public async Task<Ref.CurrentAccountReply> Account_CurrentAccountData(Ref.SessionRequest request, ServerCallContext context, string methodName)
        {
            TradeBot.Account.AccountService.v1.CurrentAccountReply response = null;
            async Task<TradeBot.Account.AccountService.v1.CurrentAccountReply> task()
            {
                response = await ClientAccount.CurrentAccountDataAsync(new TradeBot.Account.AccountService.v1.SessionRequest
                {
                    SessionId = request.SessionId
                }, context.RequestHeaders);
                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);

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
            return await Generalization.ReturnResponse(accountResponse, methodName);
        }
        public async Task<Ref.AddExchangeAccessReply> Account_AddExcengeAccess(Ref.AddExchangeAccessRequest request, ServerCallContext context, string methodName)
        {
            TradeBot.Account.AccountService.v1.AddExchangeAccessReply response = null;
            async Task<TradeBot.Account.AccountService.v1.AddExchangeAccessReply> task()
            {
                response = await ClientExc.AddExchangeAccessAsync(new TradeBot.Account.AccountService.v1.AddExchangeAccessRequest
                {
                    Secret = request.Secret,
                    SessionId = request.SessionId,
                    Code = (TradeBot.Account.AccountService.v1.ExchangeCode)request.Code,
                    ExchangeName = request.ExchangeName,
                    Token = request.Token
                }, context.RequestHeaders);
                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);
            return await Generalization.ReturnResponse(new TradeBot.Facade.FacadeService.v1.AddExchangeAccessReply { Message = response.Message, Result = (Ref.ActionCode)response.Result }, methodName);
        }
        public async Task<Ref.AllExchangesBySessionReply> Account_AllExcangesBySession(Ref.SessionRequest request, ServerCallContext context, string methodName)
        {
            TradeBot.Account.AccountService.v1.AllExchangesBySessionReply response = null;
            async Task<TradeBot.Account.AccountService.v1.AllExchangesBySessionReply> task()
            {
                response = await ClientExc.AllExchangesBySessionAsync(new TradeBot.Account.AccountService.v1.SessionRequest
                {
                    SessionId = request.SessionId
                }, context.RequestHeaders);
                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);
            return await Generalization.ReturnResponse(new TradeBot.Facade.FacadeService.v1.AllExchangesBySessionReply { Message = response.Message, Result = (Ref.ActionCode)response.Result }, methodName);
        }
        public async Task<Ref.DeleteExchangeAccessReply> Account_DeleteChangesAccess(Ref.DeleteExchangeAccessRequest request, ServerCallContext context, string methodName)
        {
            TradeBot.Account.AccountService.v1.DeleteExchangeAccessReply response = null;
            async Task<TradeBot.Account.AccountService.v1.DeleteExchangeAccessReply> task()
            {
                response = await ClientExc.DeleteExchangeAccessAsync(new TradeBot.Account.AccountService.v1.DeleteExchangeAccessRequest
                {
                    SessionId = request.SessionId,
                    Code = (TradeBot.Account.AccountService.v1.ExchangeCode)request.Code
                }, context.RequestHeaders);
                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);
            return await Generalization.ReturnResponse(new TradeBot.Facade.FacadeService.v1.DeleteExchangeAccessReply { Message = response.Message, Result = (Ref.ActionCode)response.Result }, methodName);
        }
        public async Task<Ref.ExchangeBySessionReply> Account_ExchangeBySession(Ref.ExchangeBySessionRequest request, ServerCallContext context, string methodName)
        {
            TradeBot.Account.AccountService.v1.ExchangeBySessionReply response = null;
            async Task<TradeBot.Account.AccountService.v1.ExchangeBySessionReply> task()
            {
                response = await ClientExc.ExchangeBySessionAsync(new TradeBot.Account.AccountService.v1.ExchangeBySessionRequest
                {
                    SessionId = request.SessionId,
                    Code = (TradeBot.Account.AccountService.v1.ExchangeCode)request.Code
                }, context.RequestHeaders);
                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);
            return await Generalization.ReturnResponse(new TradeBot.Facade.FacadeService.v1.ExchangeBySessionReply { Message = response.Message, Result = (Ref.ActionCode)response.Result }, methodName);
        }



    }
}
