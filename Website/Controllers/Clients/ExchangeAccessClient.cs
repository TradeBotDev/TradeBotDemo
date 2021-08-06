using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Website.Models;

namespace Website.Controllers.Clients
{
    public class ExchangeAccessClient
    {
        private static ExchangeAccess.ExchangeAccessClient client = new(GrpcChannel.ForAddress("http://localhost:5000"));

        public static AddExchangeAccessResponse AddExchangeAccess(string sessionId, AddExchangeAccessModel model)
        {
            ExchangeAccessCode exchangeAccessCode;
            switch (model.SelectExchange)
            {
                case "Bitmex":
                    exchangeAccessCode = ExchangeAccessCode.Bitmex;
                    break;
                default:
                    exchangeAccessCode = ExchangeAccessCode.Unspecified;
                    break;
            }

            var request = new AddExchangeAccessRequest{
                SessionId = sessionId,
                Code = exchangeAccessCode,
                ExchangeName = model.SelectExchange,
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

        public static ExchangeBySessionResponse ExchangeBySession()
        {
            return null;
        }
    }
}
