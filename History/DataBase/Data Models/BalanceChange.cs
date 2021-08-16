using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace History.DataBase
{
    public class BalanceChange
    {
        [Key]
        public int Id { get; set; }
        public string SessionId { get; set; }
        public DateTime Time { get; set; }
        public Balance Balance { get; set; }
    }
}
