using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auth = TradeBot.Account.AccountService.v1.Account.AccountClient;
using Exch = TradeBot.Account.AccountService.v1.ExchangeAccess.ExchangeAccessClient;
using Lic = TradeBot.Account.AccountService.v1.License.LicenseClient;
using Ref = TradeBot.Facade.FacadeService.v1;

namespace Facade
{
    public class AutorisationClass
    {
        #region Initialization
        private GrpcChannel _channel => GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("ACCOUNT_CONNECTION_STRING"));
        private GrpcChannel Channel { get => _channel; }
        private Auth _clientAccount => new Auth(Channel);
        private Auth ClientAccount
        {
            get => _clientAccount;
        }
        private Exch _clientExc => new Exch(Channel);
        private Exch ClientExc
        {
            get => _clientExc;
        }
        private Lic _clientLic => new Lic(Channel);
        private Lic ClientLic
        {
            get => _clientLic;
        }
        #endregion

        #region Account
        public async Task<Ref.LoginReply> Account_Login(Ref.LoginRequest request, string methodName)
        {
            TradeBot.Account.AccountService.v1.LoginResponse response = null;
            async Task<TradeBot.Account.AccountService.v1.LoginResponse> task()
            {
                var loginrequest = new TradeBot.Account.AccountService.v1.LoginRequest
                {
                    Email = request.Email,
                    //SaveExchangesAfterLogout = request.SaveExchangesAfterLogout,
                    //Уже этой строчки здесь нет!
                    Password = request.Password
                };
                response =await ClientAccount.LoginAsync(loginrequest);
                Log.Information("Loggin Completed With Result {@LoginResult}", response);
                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);
            return await Generalization.ReturnResponse(new Ref.LoginReply { SessionId = response.SessionId, Message = response.Message, Result = (Ref.ActionCode)response.Result }, methodName);
        }
        public async Task<Ref.LogoutReply> Account_Logout(Ref.SessionRequest request, ServerCallContext context, string methodName)
        {
            TradeBot.Account.AccountService.v1.LogoutResponse response = null;
            async Task<TradeBot.Account.AccountService.v1.LogoutResponse> task()
            {
                response = await ClientAccount.LogoutAsync(new TradeBot.Account.AccountService.v1.LogoutRequest
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
            TradeBot.Account.AccountService.v1.RegisterResponse response = null;
            async Task<TradeBot.Account.AccountService.v1.RegisterResponse> task()
            {
                response = await ClientAccount.RegisterAsync(new TradeBot.Account.AccountService.v1.RegisterRequest
                {
                    Email = request.Email,
                    Password = request.Password,
                    VerifyPassword = request.VerifyPassword
                }, context.RequestHeaders);
                Log.Information("Register Completed With Result {@LoginResult}", response);

                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);
            return await Generalization.ReturnResponse(new TradeBot.Facade.FacadeService.v1.RegisterReply { Message = response.Message, Result = (Ref.ActionCode)response.Result }, methodName);
        }
        public async Task<Ref.CurrentAccountReply> Account_CurrentAccountData(Ref.SessionRequest request, ServerCallContext context, string methodName)
        {
            TradeBot.Account.AccountService.v1.AccountDataResponse response = null;
            async Task<TradeBot.Account.AccountService.v1.AccountDataResponse> task()
            {
                response = await ClientAccount.AccountDataAsync(new TradeBot.Account.AccountService.v1.AccountDataRequest
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
        
        //public async Task<Ref.AccountDataResponse> Account_AccountData(Ref.AccountDataRequest request,ServerCallContext context, string methodName)
        //{
        //    TradeBot.Account.AccountService.v1.AccountDataResponse respone = null;
        //    async Task<TradeBot.Account.AccountService.v1.AccountDataResponse> task()
        //    {
        //        respone = await ClientAccount.AccountDataAsync(new TradeBot.Account.AccountService.v1.AccountDataRequest 
        //        { 
        //            SessionId=request.SessionId
        //        },context.RequestHeaders);
        //        await Generalization.ConnectionTester(task,methodName,request);
        //        TradeBot.Account.AccountService.v1.
        //        return await Generalization.ReturnResponse(new TradeBot.Facade.FacadeService.v1.AccountDataResponse 
        //        { 
        //            CurrentAccount = , 
        //            Message = respone.Message, 
        //            Result = (Ref.AccountActionCode)respone.Result }, methodName);
        //    }
        //}
        #endregion

        #region Excenge
        public async Task<Ref.AddExchangeAccessReply> Account_AddExcengeAccess(Ref.AddExchangeAccessRequest request, ServerCallContext context, string methodName)
        {
            TradeBot.Account.AccountService.v1.AddExchangeAccessResponse response = null;
            async Task<TradeBot.Account.AccountService.v1.AddExchangeAccessResponse> task()
            {
                response = await ClientExc.AddExchangeAccessAsync(new TradeBot.Account.AccountService.v1.AddExchangeAccessRequest
                {
                    Secret = request.Secret,
                    SessionId = request.SessionId,
                    Code = (TradeBot.Account.AccountService.v1.ExchangeAccessCode)request.Code,
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
            TradeBot.Account.AccountService.v1.AllExchangesBySessionResponse response = null;
            async Task<TradeBot.Account.AccountService.v1.AllExchangesBySessionResponse> task()
            {
                response = await ClientExc.AllExchangesBySessionAsync(new TradeBot.Account.AccountService.v1.AllExchangesBySessionRequest
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
            TradeBot.Account.AccountService.v1.DeleteExchangeAccessResponse response = null;
            async Task<TradeBot.Account.AccountService.v1.DeleteExchangeAccessResponse> task()
            {
                response = await ClientExc.DeleteExchangeAccessAsync(new TradeBot.Account.AccountService.v1.DeleteExchangeAccessRequest
                {
                    SessionId = request.SessionId,
                    Code = (TradeBot.Account.AccountService.v1.ExchangeAccessCode)request.Code
                }, context.RequestHeaders);
                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);
            return await Generalization.ReturnResponse(new TradeBot.Facade.FacadeService.v1.DeleteExchangeAccessReply { Message = response.Message, Result = (Ref.ActionCode)response.Result }, methodName);
        }
        public async Task<Ref.ExchangeBySessionReply> Account_ExchangeBySession(Ref.ExchangeBySessionRequest request, ServerCallContext context, string methodName)
        {
            TradeBot.Account.AccountService.v1.ExchangeBySessionResponse response = null;
            async Task<TradeBot.Account.AccountService.v1.ExchangeBySessionResponse> task()
            {
                response = await ClientExc.ExchangeBySessionAsync(new TradeBot.Account.AccountService.v1.ExchangeBySessionRequest
                {
                    SessionId = request.SessionId,
                    Code = (TradeBot.Account.AccountService.v1.ExchangeAccessCode)request.Code
                }, context.RequestHeaders);
                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);
            return await Generalization.ReturnResponse(new Ref.ExchangeBySessionReply { Message = response.Message, Result = (Ref.ActionCode)response.Result }, methodName);
        }
        #endregion

        #region License
        public async Task<Ref.SetLicenseResponse> License_SetLicense(Ref.SetLicenseRequest request,ServerCallContext context, string methodName)
        {
            TradeBot.Account.AccountService.v1.SetLicenseResponse response = null;
            async Task<TradeBot.Account.AccountService.v1.SetLicenseResponse> task()
            {
                response = await ClientLic.SetLicenseAsync(new TradeBot.Account.AccountService.v1.SetLicenseRequest 
                { 
                    SessionId=request.SessionId,
                    CardNumber=request.CardNumber,
                    Cvv=request.Cvv,
                    Date=request.Date,
                    Product= (TradeBot.Account.AccountService.v1.ProductCode)request.Product
                }, context.RequestHeaders);
                return response;
            }
            await Generalization.ConnectionTester(task, methodName, request);
            return await Generalization.ReturnResponse(new Ref.SetLicenseResponse { Code = (Ref.LicenseCode)response.Code, Message = response.Message }, methodName);
        }
        #endregion
    }
}
