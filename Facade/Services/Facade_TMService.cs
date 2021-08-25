using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using TradeBot.Facade.FacadeService.v1;

namespace Facade
{
    public class FacadeTMService : TradeBot.Facade.FacadeService.v1.FacadeService.FacadeServiceBase
    {
        ////TODO вынести
        #region Variables
        //private TradeBot.Account.AccountService.v1.Account.AccountClient clientAccount = new TradeBot.Account.AccountService.v1.Account.AccountClient(GrpcChannel.ForAddress("https://localhost:5000"));
        //private TradeBot.Account.AccountService.v1.ExchangeAccess.ExchangeAccessClient clientExchange = new TradeBot.Account.AccountService.v1.ExchangeAccess.ExchangeAccessClient(GrpcChannel.ForAddress("https://localhost:5000"));
        //private TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient clientRelay = new TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient(GrpcChannel.ForAddress("https://localhost:5004"));
        //private TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient clientTM = new TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient(GrpcChannel.ForAddress("https://localhost:5005"));

        //public TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient ClientTM { get => clientTM; set => clientTM = value; }
        //public TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient ClientRelay { get => clientRelay; set => clientRelay = value; }
        //public TradeBot.Account.AccountService.v1.Account.AccountClient ClientAccount { get => clientAccount; set => clientAccount = value; }

        private AutorisationClass clientAccount = new AutorisationClass();
        private HistoryClass clientHistory = new HistoryClass();
        private RelayClass clientRelay = new RelayClass();
        private TradeMarketClass clientTM = new TradeMarketClass();
        #endregion

        #region TradeMarket
        public override async Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(null, context, nameof(SubscribeBalance));
            if (response.Result.IsValid)
            {
                await clientTM.TM_SubscribeBalance(request, responseStream, context, nameof(SubscribeBalance));
            }
        }
        public override async Task Slots(SlotsRequest request, IServerStreamWriter<SlotsResponse> responseStream, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(null, context, nameof(Slots));
            if (response.Result.IsValid)
            {
                await clientTM.TM_Slots(request, responseStream, context, nameof(Slots));
            }
        }
        public override async Task SubscribeLogsTM(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(null, context, nameof(SubscribeLogsTM));
            if (response.Result.IsValid)
            {
                await clientTM.TM_SubscribeLogsTM(request, responseStream, context, nameof(SubscribeLogsTM));
            }
        }
        public async override Task<AuthenticateTokenResponse> AuthenticateToken(AuthenticateTokenRequest request, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(null, context, nameof(AuthenticateToken));
            if (response.Exception == null )
            {
                if (response.Result.IsValid)
                {
                    return await clientTM.TM_AuthenticateToken(request, nameof(AuthenticateToken));
                }
                else
                {
                    return await Generalization.ReturnResponse(new AuthenticateTokenResponse { Response = new TradeBot.Common.v1.DefaultResponse { Message = response.Result.Message } }, nameof(AuthenticateToken));
                }
            }
            else
            {
                return await Generalization.ReturnResponse(new AuthenticateTokenResponse {Message =new TradeBot.Common.v1.DefaultResponse { Message="Doesn't exist"} }, nameof(AuthenticateToken));
            }
        }
        #endregion

        #region Relay
        public override async Task SubscribeLogsRelay(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(null, context, nameof(SubscribeLogsRelay));
            if (response.Result.IsValid)
            {
                await clientRelay.Relay_SubscribeLogs(request, responseStream, context, nameof(SubscribeLogsRelay));
            }
        }
        public async override Task<DeleteOrderResponse> DeleteOrder(DeleteOrderRequest request, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(null, context, nameof(DeleteOrder));
            if (response.Exception == null)
            {
                if (response.Result.IsValid)
                {
                    return await clientRelay.Relay_DeleteOrder(context, nameof(DeleteOrder));
                }
                else
                {
                    return await Generalization.ReturnResponse(new DeleteOrderResponse { Response = new TradeBot.Common.v1.DefaultResponse { Message = response.Result.Message } }, nameof(DeleteOrder));
                }
            }
            else
            {
                return await Generalization.ReturnResponse(new DeleteOrderResponse { Response = new TradeBot.Common.v1.DefaultResponse { Message = "Doesn't exist" } }, nameof(DeleteOrder));
            }
        }
        public async override Task<SwitchBotResponse> StartBot(SwitchBotRequest request, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(null, context, nameof(StartBot));

            if (response.Exception == null)
            {
                if (response.Result.IsValid)
                {
                    return await clientRelay.Relay_StartBot(request, context, nameof(StartBot));
                }
                else
                {
                    return await Generalization.ReturnResponse(new SwitchBotResponse { Message = new TradeBot.Common.v1.DefaultResponse { Message = response.Result.Message } }, nameof(StartBot));
                }
            }
            else
            {
                return await Generalization.ReturnResponse(new SwitchBotResponse { Message = new TradeBot.Common.v1.DefaultResponse { Message = "Doesn't exist" } }, nameof(StartBot));
            }
        }
        public async override Task<StopBotResponse> StopBot(StopBotRequest request, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(null, context, nameof(StopBot));
            if (response.Exception == null)
            {
                if (response.Result.IsValid)
                {
                    return await clientRelay.Relay_stopBot(request, context, nameof(StopBot));
                }
                else
                {
                    return await Generalization.ReturnResponse(new StopBotResponse { }, nameof(StopBot));
                }
            }
            else
            {
                return await Generalization.ReturnResponse(new StopBotResponse {}, nameof(StopBot));
            }
        }
        public async override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(null, context, nameof(UpdateServerConfig));
            if (response.Exception == null)
            {
                if (response.Result.IsValid)
                {
                    return await clientRelay.Relay_UpdateServerConfig(request, context, nameof(UpdateServerConfig));
                }
                else
                {
                    return await Generalization.ReturnResponse(new UpdateServerConfigResponse { Message = new TradeBot.Common.v1.DefaultResponse { Message = response.Result.Message } }, nameof(UpdateServerConfig));
                }
            }
            else
            {
                return await Generalization.ReturnResponse(new UpdateServerConfigResponse {Message=new TradeBot.Common.v1.DefaultResponse { Message = "Doesn't exist" } }, nameof(UpdateServerConfig));
            }
        }
        #endregion

        #region Account
        public async override Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            return await clientAccount.Account_Login(request, nameof(Login));
        }
        public async override Task<LogoutResponse> Logout(LogoutRequest request, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(new IsValidSessionRequest { SessionId=request.SessionId}, context, nameof(Logout));
            if (response.Exception == null)
            {
                if (response.Result.IsValid)
                {
                    return await clientAccount.Account_Logout(request, context, nameof(Logout));
                }
                else
                {
                    return await Generalization.ReturnResponse(new LogoutResponse { Message = response.Result.Message }, nameof(Logout));
                }
            }
            else
            {
                return await Generalization.ReturnResponse(new LogoutResponse { Message = "Doesn't exist" }, nameof(Logout));
            }
        }
        public async override Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            return await clientAccount.Account_Register(request, context, nameof(Register));
        }
        public async override Task<AddExchangeAccessResponse> AddExchangeAccess(AddExchangeAccessRequest request, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(new IsValidSessionRequest {SessionId=request.SessionId }, context, nameof(AddExchangeAccess));
            if (response.Exception == null)
            {
                if (response.Result.IsValid)
                {
                    return await clientAccount.Account_AddExcengeAccess(request, context, nameof(AddExchangeAccess));
                }
                else
                {
                    return await Generalization.ReturnResponse(new AddExchangeAccessResponse { Message = response.Result.Message }, nameof(AddExchangeAccess));
                }
            }
            else
            {
                return await Generalization.ReturnResponse(new AddExchangeAccessResponse { Message = "Doesn't exist" }, nameof(AddExchangeAccess));
            }
        }
        public async override Task<AllExchangesBySessionResponse> AllExchangesBySession(AllExchangesBySessionRequest request, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(new IsValidSessionRequest { SessionId = request.SessionId }, context, nameof(AllExchangesBySession));
            if (response.Exception == null)
            {
                if (response.Result.IsValid)
                {
                    return await clientAccount.Account_AllExcangesBySession(request, context, nameof(AllExchangesBySession));
                }
                else
                {
                    return await Generalization.ReturnResponse(new AllExchangesBySessionResponse { Message = response.Result.Message }, nameof(AllExchangesBySession));
                }
            }
            else
            {
                return await Generalization.ReturnResponse(new AllExchangesBySessionResponse { Message = "Doesn't exist" }, nameof(AllExchangesBySession));
            }
        }
        public async override Task<DeleteExchangeAccessResponse> DeleteExchangeAccess(DeleteExchangeAccessRequest request, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(new IsValidSessionRequest { SessionId = request.SessionId }, context, nameof(DeleteExchangeAccess));
            if (response.Exception == null)
            {
                if (response.Result.IsValid)
                {
                    return await clientAccount.Account_DeleteChangesAccess(request, context, nameof(DeleteExchangeAccess));
                }
                else
                {
                    return await Generalization.ReturnResponse(new DeleteExchangeAccessResponse { Message = response.Result.Message }, nameof(DeleteExchangeAccess));
                }
            }
            else
            {
                return await Generalization.ReturnResponse(new DeleteExchangeAccessResponse { Message = "Doesn't exist" }, nameof(DeleteExchangeAccess));
            }
        }
        public async override Task<ExchangeBySessionResponse> ExchangeBySession(ExchangeBySessionRequest request, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(new IsValidSessionRequest { SessionId = request.SessionId }, context, nameof(ExchangeBySession));
            if (response.Exception == null)
            {
                if (response.Result.IsValid)
                {
                    return await clientAccount.Account_ExchangeBySession(request, context, nameof(ExchangeBySession));
                }
                else
                {
                    return await Generalization.ReturnResponse(new ExchangeBySessionResponse { Message = response.Result.Message }, nameof(ExchangeBySession));
                }
            }
            else
            {
                return await Generalization.ReturnResponse(new ExchangeBySessionResponse { Message = "Doesn't exist" }, nameof(ExchangeBySession));
            }
        }
        public async override Task<AccountDataResponse> AccountData(AccountDataRequest request, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(new IsValidSessionRequest { SessionId = request.SessionId }, context, nameof(AccountData));
            if (response.Exception == null)
            {
                if (response.Result.IsValid)
                {
                    return await clientAccount.Account_AccountData(request, context, nameof(AccountData));
                }
                else
                {
                    return await Generalization.ReturnResponse(new AccountDataResponse { Message = response.Result.Message }, nameof(AccountData));
                }
            }
            else
            {
                return await Generalization.ReturnResponse(new AccountDataResponse { Message = "Doesn't exist" }, nameof(AccountData));
            }
        }
        public async override Task<SetLicenseResponse> SetLicense(SetLicenseRequest request, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(new IsValidSessionRequest { SessionId = request.SessionId }, context, nameof(SetLicense));
            if (response.Exception == null)
            {
                if (response.Result.IsValid)
                {
                    return await clientAccount.Account_SetLicense(request, context, nameof(SetLicense));
                }
                else
                {
                    return await Generalization.ReturnResponse(new SetLicenseResponse { Message = response.Result.Message }, nameof(SetLicense));
                }
            }
            else
            {
                return await Generalization.ReturnResponse(new SetLicenseResponse { Message = "Doesn't exist"}, nameof(SetLicense));
            }
        }
        public async override Task<CheckLicenseResponse> CheckLicense(CheckLicenseRequest request, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(new IsValidSessionRequest { SessionId = request.SessionId }, context, nameof(CheckLicense));
            if (response.Exception == null)
            {
                if (response.Result.IsValid)
                {
                    return await clientAccount.Account_CheckLicense(request, context, nameof(CheckLicense));
                }
                else
                {
                    return await Generalization.ReturnResponse(new CheckLicenseResponse { Message = response.Result.Message }, nameof(CheckLicense));
                }
            }
            else
            {
                return await Generalization.ReturnResponse(new CheckLicenseResponse {Message="Doesn't exist"}, nameof(CheckLicense));
            }
        }
        #endregion

        #region History
        public async override Task SubscribeEvents(SubscribeEventsRequest request, IServerStreamWriter<SubscribeEventsResponse> responseStream, ServerCallContext context)
        {
            var response = clientAccount.Account_IsValidSession(new IsValidSessionRequest {SessionId=request.Sessionid }, context, nameof(SubscribeEvents));
            if (response.Result.IsValid)
            {
                await clientHistory.History_SubscribeEvents(request, responseStream, context);
            }
        }

        #endregion

    }
}
