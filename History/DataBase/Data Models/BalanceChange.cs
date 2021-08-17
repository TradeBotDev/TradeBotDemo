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
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string SessionId { get; set; }
        public DateTime Time { get; set; }
        public int BalanceId { get; set; }
        public BalanceWrapper Balance { get; set; }
    }
}