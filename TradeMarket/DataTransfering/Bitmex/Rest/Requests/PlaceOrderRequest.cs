using Bitmex.Client.Websocket.Responses.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Responses;

namespace TradeMarket.DataTransfering.Bitmex.Rest.Requests
{
    public class PlaceOrderRequest : BitmexRestfulRequest
    {
        public PlaceOrderRequest(string key, string secret, Order order) 
            : base(key, secret, HttpMethod.Post, /*"api/v1" +*/ "/order", JsonSerializer.Serialize(new Dictionary<string, string>
            {
                {"ordType","Limit" },
                {"symbol",order.Symbol },
                {"orderQty",order.OrderQty.ToString() },
                {"price",order.Price.ToString() }
            })) { }
    }
}
