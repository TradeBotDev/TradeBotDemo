using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Website.Models;

namespace Website.Controllers.Clients
{
    public class ExchangeAccessClient
    {
        private static ExchangeAccess.ExchangeAccessClient client = new(AccountServiceConnection.GetConnection());

        public static async Task<AddExchangeAccessResponse> AddExchangeAccess(string sessionId, AddExchangeAccessModel model)
        {
            var request = new AddExchangeAccessRequest
            {
                SessionId = sessionId,
                Code = model.ExchangeCode,
                ExchangeName = model.ExchangeCode.ToString(),
                Token = model.Token,
                Secret = model.Secret
            };

            return await client.AddExchangeAccessAsync(request);
        }

        public static async Task<AllExchangesBySessionResponse> AllExchangesBySession(string sessionId)
        {
            var request = new AllExchangesBySessionRequest
            {
                SessionId = sessionId
            };

            return await client.AllExchangesBySessionAsync(request);
        }

        public static async Task<DeleteExchangeAccessResponse> DeleteExchangeAccess(string sessionId, ExchangeAccessCode code)
        {
            var request = new DeleteExchangeAccessRequest
            {
                SessionId = sessionId,
                Code = code
            };

            return await client.DeleteExchangeAccessAsync(request);
        }

        public static async Task<ExchangeBySessionResponse> ExchangeBySession(string sessionId, ExchangeAccessCode code)
        {
            var request = new ExchangeBySessionRequest
            {
                SessionId = sessionId,
                Code = code
            };

            return await client.ExchangeBySessionAsync(request);
        }
    }
}
