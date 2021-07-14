using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Algorithm.DataService.v1;
using TradeBot.Common.v1;
using System.Threading;

namespace Algorithm.Services
{
    public class DataReceiverService :  DataService.DataServiceBase
    {
        public override async Task<AddOrderResponse> AddOrder(IAsyncStreamReader<AddOrderRequest> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var order = requestStream.Current;
                DataCollector.orders.Add(order.Order);
                Console.WriteLine("Got something");
            }
            AddOrderResponse response = new AddOrderResponse();
            return response;
        }
    }
}

