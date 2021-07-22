using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace Account.Models
{
    public class ExchangeAccess
    {
        public int ExchangeAccessId { get; set; }

        public ExchangeCode Code { get; set; }

        public string Name { get; set; }

        public string Token { get; set; }

        public int Secret { get; set; }

        public Account Account { get; set; }
    }
}
