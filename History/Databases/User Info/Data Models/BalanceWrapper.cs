using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace History.DataBase.Data_Models
{
    public class BalanceWrapper
    {
        [Key]
        public int BalanceId { get; set; }
        public string Currency { get; set; }
        public string Value { get; set; }
        public BalanceChange BalanceChange { get; set; } 
    }
}
