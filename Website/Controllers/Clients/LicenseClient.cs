using Grpc.Net.Client;
using TradeBot.License.LicenseService.v1;

namespace Website.Controllers.Clients
{
    public static class LicenseClient
    {
        private static License.LicenseClient client = new(GrpcChannel.ForAddress("http://localhost:5007"));

        public static SetLicenseResponse SetLicense(string sessionId, ProductCode product)
        {
            int accountId = AccountServiceClient.AccountData(sessionId).CurrentAccount.AccountId;
            var request = new SetLicenseRequest
            {
                AccountId = accountId,
                Product = product
            };

            return client.SetLicense(request);
        }

        public static CheckLicenseResponse CheckLicense(string sessionId, ProductCode product)
        {
            int accountId = AccountServiceClient.AccountData(sessionId).CurrentAccount.AccountId;
            var request = new CheckLicenseRequest
            {
                AccountId = accountId,
                Product = product
            };

            return client.CheckLicense(request);
        }
    }
}
