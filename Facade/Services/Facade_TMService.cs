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
        //TODO вынести
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
            await clientTM.TM_SubscribeBalance(request, responseStream, context, nameof(SubscribeBalance));
        }
        public override async Task Slots(SlotsRequest request, IServerStreamWriter<SlotsResponse> responseStream, ServerCallContext context)
        {
            await clientTM.TM_Slots(request, responseStream, context, nameof(SubscribeBalance));
        }
        public override async Task SubscribeLogsTM(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            await clientTM.TM_SubscribeLogsTM(request, responseStream, context, nameof(SubscribeLogsTM));
        }
        public async override Task<AuthenticateTokenResponse> AuthenticateToken(AuthenticateTokenRequest request, ServerCallContext context)
        {
            return await clientTM.TM_AuthenticateToken(request, nameof(AuthenticateToken));
        }
        #endregion

        #region Relay
        public override async Task SubscribeLogsRelay(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            await clientRelay.Relay_SubscribeLogs(request, responseStream, context, nameof(SubscribeLogsRelay));
        }
        public async override Task<DeleteOrderResponse> DeleteOrder(DeleteOrderRequest request, ServerCallContext context)
        {
            return await clientRelay.Relay_DeleteOrder(context, nameof(DeleteOrder));
        }
        public async override Task<SwitchBotResponse> StartBot(SwitchBotRequest request, ServerCallContext context)
        {
            return await clientRelay.Relay_StartBot(request, context, nameof(StartBot));

        }
        public async override Task<StopBotResponse> StopBot(StopBotRequest request, ServerCallContext context)
        {
            return await clientRelay.Relay_stopBot(request, context, nameof(StopBot));
        }
        public async override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request, ServerCallContext context)
        {
            return await clientRelay.Relay_UpdateServerConfig(request, context, nameof(UpdateServerConfig));
        }
        #endregion

        #region Account
        public async override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            return await clientAccount.Account_Login(request, nameof(Login));
        }
        public async override Task<LogoutReply> Logout(SessionRequest request, ServerCallContext context)
        {
            return await clientAccount.Account_Logout(request, context, nameof(Logout));
        }
        public async override Task<RegisterReply> Register(RegisterRequest request, ServerCallContext context)
        {
            return await clientAccount.Account_Register(request, context, nameof(Register));
        }
        public async override Task<CurrentAccountReply> CurrentAccountData(SessionRequest request, ServerCallContext context)
        {
            return await clientAccount.Account_CurrentAccountData(request, context, nameof(CurrentAccountData));

        }
        public async override Task<AddExchangeAccessReply> AddExchangeAccess(AddExchangeAccessRequest request, ServerCallContext context)
        {
            return await clientAccount.Account_AddExcengeAccess(request, context, nameof(AddExchangeAccess));

        }
        public async override Task<AllExchangesBySessionReply> AllExchangesBySession(SessionRequest request, ServerCallContext context)
        {
            return await clientAccount.Account_AllExcangesBySession(request, context, nameof(AllExchangesBySession));
        }
        public async override Task<DeleteExchangeAccessReply> DeleteExchangeAccess(DeleteExchangeAccessRequest request, ServerCallContext context)
        {
            return await clientAccount.Account_DeleteChangesAccess(request, context, nameof(DeleteExchangeAccess));

        }
        public async override Task<ExchangeBySessionReply> ExchangeBySession(ExchangeBySessionRequest request, ServerCallContext context)
        {
            return await clientAccount.Account_ExchangeBySession(request, context, nameof(ExchangeBySession));
        }
        #endregion
        //public async override Task<AccountDataResponse> AccountData(AccountDataRequest request, ServerCallContext context)
        //{
        //    return await clientAccount.License_SetLicense(request,context,nameof(AccountData));
        //}

        #region History
        public async override Task SubscribeEvents(SubscribeEventsRequest request, IServerStreamWriter<SubscribeEventsResponse> responseStream, ServerCallContext context)
        {
            await clientHistory.History_SubscribeEvents(request, responseStream, context);
        }

        #endregion
    }
}
