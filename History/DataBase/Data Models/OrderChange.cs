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
    public class OrderChange
    {
        [Key]
        public int Id { get; set; }
        public Order Order { get; set; }
        public string SessionId { get; set; }
        public ChangesType ChangesType { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }

    }
}
