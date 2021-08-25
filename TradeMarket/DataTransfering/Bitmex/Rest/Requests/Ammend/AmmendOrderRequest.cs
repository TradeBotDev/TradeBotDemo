using Bitmex.Client.Websocket.Responses.Orders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace TradeMarket.DataTransfering.Bitmex.Rest.Requests.Ammend
{
    public class AmmendOrderRequest : RestfulRequest<Order>
    {
        public AmmendOrderRequest(string key, string secret, AmmendOrderDTO dto) : base(key, secret, HttpMethod.Put, "/api/v1/order", ContextConverter.Convert(dto))
        {
        }
    }
}
