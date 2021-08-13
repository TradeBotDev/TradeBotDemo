using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.Models
{
    public class License
    {
        public int LicenseId { get; set; }

        public Account Account { get; set; }

        public ProductCode Product { get; set; }
    }
}
