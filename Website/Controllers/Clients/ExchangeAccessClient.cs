using Grpc.Net.Client;
using TradeBot.Account.AccountService.v1;
using Website.Models;

namespace Website.Controllers.Clients
{
    public class ExchangeAccessClient
    {
        private static ExchangeAccess.ExchangeAccessClient client = new(GrpcChannel.ForAddress("http://localhost:5000"));

        public static AddExchangeAccessResponse AddExchangeAccess(string sessionId, AddExchangeAccessModel model)
        {
            var request = new AddExchangeAccessRequest
            {
                SessionId = sessionId,
                Code = model.ExchangeCode,
                ExchangeName = model.ExchangeCode.ToString(),
                Token = model.Token,
                Secret = model.Secret
            };

            return client.AddExchangeAccess(request);
        }

        public static AllExchangesBySessionResponse AllExchangesBySession(string sessionId)
        {
            var request = new AllExchangesBySessionRequest
            {
                SessionId = sessionId
            };

            return client.AllExchangesBySession(request);
        }

        public static DeleteExchangeAccessResponse DeleteExchangeAccess(string sessionId, ExchangeAccessCode code)
        {
            var request = new DeleteExchangeAccessRequest
            {
                SessionId = sessionId,
                Code = code
            };

            return client.DeleteExchangeAccess(request);
        }

        public static ExchangeBySessionResponse ExchangeBySession(string sessionId, ExchangeAccessCode code)
        {
            var request = new ExchangeBySessionRequest
            {
                SessionId = sessionId,
                Code = code
            };

            return client.ExchangeBySession(request);
        }
    }
}
