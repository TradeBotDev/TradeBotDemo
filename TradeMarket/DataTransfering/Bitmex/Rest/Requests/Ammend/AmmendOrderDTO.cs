using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering.Bitmex.Rest
{
    public class AmmendOrderDTO
    {
        [JsonProperty("orderID")]
        public string Id { get; set; }
        
        [JsonProperty("price")]
        public double? Price { get; set; }

        [JsonProperty("qty")]
        public long? Quantity { get; set; }

        [JsonProperty("LeavesQty")]
        public long? LeavesQuantity { get; set; }
    }
}
