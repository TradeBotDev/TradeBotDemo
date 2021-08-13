using Grpc.Net.Client;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Website.Models.Authorization;

namespace Website.Controllers.Clients
{
    public static class AccountServiceClient
    {
        private static Account.AccountClient client = new(AccountServiceConnection.GetConnection());

        public static async Task<LoginResponse> Login(LoginModel model)
        {
            var request = new LoginRequest
            {
                Email = model.Email,
                Password = model.Password
            };
            return await client.LoginAsync(request);
        }

        public static async Task<LogoutResponse> Logout(string sessionId, bool saveExchangeAccesses)
        {
            var request = new LogoutRequest
            {
                SessionId = sessionId,
                SaveExchangeAccesses = saveExchangeAccesses
            };
            return await client.LogoutAsync(request);
        }

        public static async Task<RegisterResponse> Register(RegisterModel model)
        {
            var request = new RegisterRequest
            {
                Email = model.Email,
                Password = model.Password,
                VerifyPassword = model.VerifyPassword
            };
            return await client.RegisterAsync(request);
        }

        public static async Task<IsValidSessionResponse> IsValidSession(string sessionId)
        {
            var request = new IsValidSessionRequest
            {
                SessionId = sessionId
            };
            return await client.IsValidSessionAsync(request);
        }

        public static async Task<AccountDataResponse> AccountData(string sessionId)
        {
            var request = new AccountDataRequest
            {
                SessionId = sessionId
            };
            return await client.AccountDataAsync(request);
        }
    }
}
