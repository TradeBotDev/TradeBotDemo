using TradeBot.License.LicenseService.v1;

namespace LicenseGRPC.Models
{
    public class License
    {
        public int LicenseId { get; set; }

        public int AccountId { get; set; }

        public string Key { get; set; }

        public ProductCode Product { get; set; }
    }
}
