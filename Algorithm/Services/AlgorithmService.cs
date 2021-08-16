using Algorithm.DataManipulation;

using Grpc.Core;
using Serilog;
using System;
using System.Threading.Tasks;
using TradeBot.Algorithm.AlgorithmService.v1;

namespace Algorithm.Services
{

    public class AlgorithmService : TradeBot.Algorithm.AlgorithmService.v1.AlgorithmService.AlgorithmServiceBase
    {
        public override async Task<AddOrderResponse> AddOrder(IAsyncStreamReader<AddOrderRequest> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var order = requestStream.Current;
                StorageOfAlgorithms.SendNewOrderToAllAlgos(order.Order, context.RequestHeaders);
            }

            return new AddOrderResponse();
        }
        public override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request, ServerCallContext context)
        {
            Log.Information("{@Where}: Config Update has been requested", "Algorithm");
            var settings = request.Request;
            StorageOfAlgorithms.SendNewConfig(context.RequestHeaders, settings);
            return Task.FromResult(new UpdateServerConfigResponse());
        }
    }
}
