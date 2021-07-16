using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common;
using System.Threading;
using TradeBot.Algorithm.AlgorithmService.v1;

namespace Algorithm.Services
{

    public class AlgorithmService : TradeBot.Algorithm.AlgorithmService.v1.AlgorithmService.AlgorithmServiceBase
    {
        DataCollector collector = new DataCollector();
        public override async Task<AddOrderResponse> AddOrder(IAsyncStreamReader<AddOrderRequest> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var order = requestStream.Current;
                collector.orders.Add(order.Order);
                Console.WriteLine("Got " + order.Order.Id);
            }
            return new AddOrderResponse();
        }

        public override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request, ServerCallContext context)
        {           
            Console.WriteLine("Config Updated");
            return Task.FromResult(new UpdateServerConfigResponse());
        }
    }
}
