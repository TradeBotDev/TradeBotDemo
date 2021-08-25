using Bitmex.Client.Websocket.Responses.Orders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Responses;

namespace TradeMarket.DataTransfering.Bitmex.Rest.Requests.Place
{
    public class PlaceOrderRequest : RestfulRequest<Order>
    {
        public PlaceOrderRequest(string key, string secret, Order order) 
            : base(key, secret, HttpMethod.Post, "/api/v1/order", 
                  ContextConverter.Convert(order)) {
        }
    }
}
