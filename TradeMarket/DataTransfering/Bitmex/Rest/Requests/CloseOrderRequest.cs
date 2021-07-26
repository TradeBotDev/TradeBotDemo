using Bitmex.Client.Websocket.Responses.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using System.Threading.Tasks;
using Utf8Json;

namespace TradeMarket.DataTransfering.Bitmex.Rest.Requests
{
    /// <summary>
    /// Нереализовано
    /// </summary>
    public class CloseOrderRequest : BitmexRestfulRequest
    {
        //todo тут лишняя сериализация
        public CloseOrderRequest(string key, string secret, Order order)
            : base(key, secret, HttpMethod.Post, "/order", JsonSerializer.ToJsonString(order)) { }
    }
}
