using Google.Protobuf.WellKnownTypes;
using History.DataBase.Data_Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace History.DataBase
{
    public class BalanceChange
    {
        public int BalanceChangeId { get; set; }
        public DateTime Time { get; set; }
        public int BalanceId { get; set; }
        public BalanceWrapper Balance { get; set; }
        public string UserId { get; set; }
    }
}