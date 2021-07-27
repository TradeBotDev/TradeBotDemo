using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Bitmex.Client.Websocket.Responses.Orders
{
    [DebuggerDisplay("Order: {Symbol}, {OrderQty}. {Price}")]
    public class Order
    {
        [JsonProperty("orderID")]
        [DataMember(Name = "orderID")]
        public string OrderId { get; set; }

        [JsonProperty("clOrdID")]
        [DataMember(Name = "clOrdID")]
        public string ClOrdId { get; set; }


        [JsonProperty("clOrdLinkID")]
        [DataMember(Name = "clOrdLinkID")]
        public string ClOrdLinkId {get; set; }

        public long? Account { get; set; }
        public string Symbol { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("side")]
        public BitmexSide Side { get; set; }

        public double? SimpleOrderQty { get; set; }
        [JsonProperty("orderQty")]
        public long? OrderQty {get; set; }
        [JsonProperty("price")]
        public double? Price { get; set; }

        [JsonProperty("displayQty")]
        public long? DisplayQty { get; set; }
        
        [JsonProperty("stopPx")]
        public double? StopPx { get; set; }

        
        [JsonProperty("pegOffsetValue")]
        public double? PegOffsetValue { get; set; }
        
        [JsonProperty("pegPriceType")]
        public string PegPriceType { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
        
        [JsonProperty("settlCurrency")]
        public string SettlCurrency { get; set; }

        
        [JsonProperty("ordType")]
        public string OrdType { get; set; }
        
        [JsonProperty("timeInForce")]
        public string TimeInForce { get; set; }
        
        [JsonProperty("execInst")]
        public string ExecInst { get; set; }
        
        [JsonProperty("contingencyType")]
        public string ContingencyType { get; set; }
        
        [JsonProperty("exDestination")]
        public string ExDestination { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("ordStatus")]
        public OrderStatus OrdStatus { get; set; }
        
        [JsonProperty("triggered")]
        public string Triggered { get; set; }

        [JsonProperty("WorkingIndicator")]
        public bool? WorkingIndicator { get; set; }
        
        [JsonProperty("OrdRejReason")]
        public string OrdRejReason { get; set; }
        
        [JsonProperty("simpleLeavesQty")]
        public double? SimpleLeavesQty { get; set; }
        
        [JsonProperty("leavesQty")]
        public long? LeavesQty { get; set; }
        
        [JsonProperty("simpleCumQty")]
        public double? SimpleCumQty { get; set; }
        
        [JsonProperty("cumQty")]
        public long? CumQty { get; set; }
        
        [JsonProperty("avgPx")]
        public double? AvgPx { get; set; }
        
        [JsonProperty("multiLegReportingType")]
        public string MultiLegReportingType { get; set; }
        
        [JsonProperty("text")]
        public string Text {get; set; }

        
        [JsonProperty("transactTime")]
        public DateTime? TransactTime { get; set; }
        
        [JsonProperty("timestamp")]
        public DateTime? Timestamp { get; set; }

    }
}
