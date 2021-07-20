﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Account.Models
{
    public class TradeMarket
    {
        public int TradeMarketId { get; set; }

        public int AccountId { get; set; }

        public string Name { get; set; }

        public string Token { get; set; }

        public int Secret { get; set; }
    }
}
