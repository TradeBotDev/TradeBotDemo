using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.License.LicenseService.v1;

namespace Website.Controllers.Clients
{
    public static class LicenseClient
    {
        private static License.LicenseClient client = new(GrpcChannel.ForAddress("http://localhost:5007"));

        public static SetLicenseResponse SetLicense(string sessionId, ProductCode productCode)
        {
            int accountId = AccountServiceClient.AccountData(sessionId).CurrentAccount.AccountId;
            var request = new SetLicenseRequest
            {
                AccountId = accountId,
                Product = productCode
            };

            return client.SetLicense(request);
        }

        public static CheckLicenseResponse CheckLicense()
        {
            return null;
        }
    }
}
