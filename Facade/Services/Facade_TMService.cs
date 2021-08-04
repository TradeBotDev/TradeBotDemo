using Grpc.Core;
using System.Threading.Tasks;
using TradeBot.Facade.FacadeService.v1;

namespace Facade
{
    public class FacadeTMService : TradeBot.Facade.FacadeService.v1.FacadeService.FacadeServiceBase
    {
        //TODO вынести
        //private TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient clientTM = new TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient(GrpcChannel.ForAddress("https://localhost:5005"));
        //private TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient Client = new TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient(GrpcChannel.ForAddress("https://localhost:5004"));
        //private TradeBot.Account.AccountService.v1.Account.AccountClient clientAccount = new TradeBot.Account.AccountService.v1.Account.AccountClient(GrpcChannel.ForAddress("https://localhost:5000"));
        //private TradeBot.Account.AccountService.v1.ExchangeAccess.ExchangeAccessClient clientExchange = new TradeBot.Account.AccountService.v1.ExchangeAccess.ExchangeAccessClient(GrpcChannel.ForAddress("https://localhost:5000"));
        private TradeMarketClass tradeMarketClient;
        private RelayClass relayClient;
        private AutorisationClass autorisationClient;
        public FacadeTMService()
        {
            tradeMarketClient = new TradeMarketClass();
            relayClient = new RelayClass();
            autorisationClient = new AutorisationClass();
        }
       
       
        #region TradeMarket
        public override async Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {
            await tradeMarketClient.SubscribeBalanceTradeMarket(request, responseStream, context);
        }

        public override async Task Slots(SlotsRequest request, IServerStreamWriter<SlotsResponse> responseStream, ServerCallContext context)
        {
            await tradeMarketClient.SlotsTradeMarket(request, responseStream, context);

        }

        public override async Task SubscribeLogsTM(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            await tradeMarketClient.SubscribeLogsTradeMarket(request, responseStream, context);

        }

        public override Task<AuthenticateTokenResponse> AuthenticateToken(AuthenticateTokenRequest request, ServerCallContext context)
        {
            return tradeMarketClient.AuthenticationTokenTradeMarket(request, context);

        }



        #endregion

        #region Relay
        public override async Task SubscribeLogsRelay(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            await relayClient.SubscribeLogsRel(request, responseStream, context);
        }



        public override Task<SwitchBotResponse> SwitchBot(SwitchBotRequest request, ServerCallContext context)
        {
            return relayClient.SwitchBotRel(request, context);
        }



        public override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request, ServerCallContext context)
        {
            return relayClient.UpdateServerConfigRel(request, context);
        }


        #endregion

        #region Account
        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            return autorisationClient.LoginAuth(request, context);
        }

        public override Task<LogoutReply> Logout(SessionRequest request, ServerCallContext context)
        {
            return autorisationClient.LogoutAuth(request, context);
        }

        public override Task<RegisterReply> Register(RegisterRequest request, ServerCallContext context)
        {
            return autorisationClient.RegisterAuth(request, context);
        }

        public override Task<SessionReply> IsValidSession(SessionRequest request, ServerCallContext context)
        {
            return autorisationClient.IsValidSessionAuth(request, context);
        }

        public override Task<CurrentAccountReply> CurrentAccountData(SessionRequest request, ServerCallContext context)
        {
            return autorisationClient.CurrentAccountDataAuth(request, context);
        }

        public override Task<AddExchangeAccessReply> AddExchangeAccess(AddExchangeAccessRequest request, ServerCallContext context)
        {
            return autorisationClient.AddExchangeAccessAuth(request, context);
        }

        public override Task<AllExchangesBySessionReply> AllExchangesBySession(SessionRequest request, ServerCallContext context)
        {
            return autorisationClient.AllExchangesBySessionAuth(request, context);
        }

        public override Task<DeleteExchangeAccessReply> DeleteExchangeAccess(DeleteExchangeAccessRequest request, ServerCallContext context)
        {
            return autorisationClient.DeleteExchangeAccessAuth(request, context);
        }

        public override Task<ExchangeBySessionReply> ExchangeBySession(ExchangeBySessionRequest request, ServerCallContext context)
        {
            return autorisationClient.ExchangeBySessionAuth(request, context);
        }

        #endregion


    }

}
