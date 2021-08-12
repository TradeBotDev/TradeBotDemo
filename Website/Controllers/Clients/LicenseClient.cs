using Grpc.Net.Client;
using TradeBot.Account.AccountService.v1;
using Website.Models;

namespace Website.Controllers.Clients
{
    public static class LicenseClient
    {
        private static License.LicenseClient client = new(GrpcChannel.ForAddress("http://host.docker.internal:5000"));

        public static SetLicenseResponse SetLicense(string sessionId, ProductCode product, CreditCardModel model)
        {
            var request = new SetLicenseRequest
            {
                SessionId = sessionId,
                Product = product,
                CardNumber = model.CardNumber,
                Date = model.Date,
                Cvv = model.CVV
            };
            return client.SetLicense(request);
        }

        public static CheckLicenseResponse CheckLicense(string sessionId, ProductCode product)
        {
            if (sessionId == null) sessionId = "";
            var request = new CheckLicenseRequest
            {
                SessionId = sessionId,
                Product = product
            };
            return client.CheckLicense(request);
        }
    }
}
