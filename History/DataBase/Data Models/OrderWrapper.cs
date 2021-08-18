using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace History.DataBase.Data_Models
{
    public class OrderWrapper
    {
        [Key]
        public int OrderId { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public string OrderIdOnTM { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public OrderStatus Status { get; set; }
        public OrderType Type { get; set; }
        public OrderChange OrderChange { get; set; }
    }

    public enum OrderStatus
    {
        ORDER_STATUS_UNSPECIFIED,
        ORDER_STATUS_OPEN,
        ORDER_STATUS_CLOSED
    }

    public enum OrderType
    {
        ORDER_TYPE_UNSPECIFIED,
        ORDER_TYPE_SELL,
        ORDER_TYPE_BUY
    }
}
