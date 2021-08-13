using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Website.Models;

namespace Website.Controllers.Clients
{
    public static class LicenseClient
    {
        private static License.LicenseClient client = new(AccountServiceConnection.GetConnection());

        public static async Task<SetLicenseResponse> SetLicense(string sessionId, ProductCode product, CreditCardModel model)
        {
            var request = new SetLicenseRequest
            {
                SessionId = sessionId,
                Product = product,
                CardNumber = model.CardNumber,
                Date = model.Date,
                Cvv = model.CVV
            };
            return await client.SetLicenseAsync(request);
        }

        public static async Task<CheckLicenseResponse> CheckLicense(string sessionId, ProductCode product)
        {
            if (sessionId == null) sessionId = "";
            var request = new CheckLicenseRequest
            {
                SessionId = sessionId,
                Product = product
            };
            return await client.CheckLicenseAsync(request);
        }
    }
}
