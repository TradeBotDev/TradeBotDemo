using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace TradeBot.Common.v1
{
    public partial class Order
    {
        [Key]
        int DBId { get; set; }
    }
}
