using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace Website.Models
{
    public class AccountPageModel
    {
        public string Email { get; set; }

        public RepeatedField<ExchangeAccessInfo> Exchanges { get; set; }
    }
}
