using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Website.Models;

namespace Website.Controllers.Clients
{
    public static class LicenseClient
    {
        // Клиент сервиса лицензий для того, чтобы можно было получить к нему доступ.
        private static License.LicenseClient client = new(AccountServiceConnection.GetConnection());

        // Метод установки лицензии для пользователя по Id сессии.
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

        // Метод проверки на то, есть ли лицензия у текущего пользователя.
        public static async Task<CheckLicenseResponse> CheckLicense(string sessionId, ProductCode product)
        {
            // Если Id сессии является null, в запросе отправляется просто пустая строка (gRPC не умеет пересылать null).
            // В таком случае сработает валидация уже в самом сервисе и вернется сообщение об ошибке.
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
