using TradeBot.License.LicenseService.v1;

namespace LicenseGRPC.Models
{
    // Описание таблицы с лицензиями пользователей.
    public class License
    {
        public int LicenseId { get; set; }

        public int AccountId { get; set; }

        public ProductCode Product { get; set; }
    }
}
