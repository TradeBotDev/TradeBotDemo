using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
