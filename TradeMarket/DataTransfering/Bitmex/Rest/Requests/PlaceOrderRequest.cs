using Bitmex.Client.Websocket.Responses.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Responses;
using Utf8Json;

namespace TradeMarket.DataTransfering.Bitmex.Rest.Requests
{
    public class PlaceOrderRequest : BitmexRestfulRequest
    {
        public PlaceOrderRequest(string key, string secret, Order order) 
            : base(key, secret, HttpMethod.Post, new Uri("/order", UriKind.Relative), JsonSerializer.ToJsonString(order)) { }
    }
}
