using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering.Bitmex.Rest.Requests
{
    public class DeleteOrderRequest : BitmexRestfulRequest
    {
        public DeleteOrderRequest(string key, string secret, string id) : base(key, secret, HttpMethod.Delete, "/api/v1/order", JsonSerializer.Serialize(new Dictionary<string, string> { {"orderID",id} }))
        {
        }
    }
}
