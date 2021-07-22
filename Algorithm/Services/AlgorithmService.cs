using Algorithm.DataManipulation;

using Grpc.Core;
using System;
using System.Threading.Tasks;
using TradeBot.Algorithm.AlgorithmService.v1;

namespace Algorithm.Services
{

    public class AlgorithmService : TradeBot.Algorithm.AlgorithmService.v1.AlgorithmService.AlgorithmServiceBase
    {
        //TODO: разобраться с импортами и их конфликтами, или нейминг поменять
        public override async Task<AddOrderResponse> AddOrder(IAsyncStreamReader<AddOrderRequest> requestStream, ServerCallContext context)
        {
            Console.WriteLine("Listening to the Relay...");
            while (await requestStream.MoveNext())
            {
                var order = requestStream.Current;
                DataCollector.Orders.Add(order.Order);
                Console.WriteLine("Got " + order.Order.Id + "   " + order.Order.Price);
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
