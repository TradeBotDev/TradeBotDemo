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
    public class OrderChange
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public OrderWrapper Order { get; set; }
        public string SessionId { get; set; }
        public ChangesType ChangesType { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
        public string SlotName { get; set; }
    }
    public enum ChangesType
    {
        CHANGES_TYPE_UNDEFIEND,
        CHANGES_TYPE_PARTITIAL,
        CHANGES_TYPE_UPDATE,
        CHANGES_TYPE_INSERT,
        CHANGES_TYPE_DELETE
    }
}
