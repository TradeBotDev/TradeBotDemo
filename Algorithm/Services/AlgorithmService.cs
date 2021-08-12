﻿using Algorithm.DataManipulation;

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
            Log.Information("Listening to Relay...");
            while (await requestStream.MoveNext())
            {
                var order = requestStream.Current;
                StorageOfAlgorithms.
                //DataCollector.Orders.Add(order.Order);
               // DataCollector.metaData = context.RequestHeaders;
            }

            return new AddOrderResponse();
        }
        public override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request, ServerCallContext context)
        {
            var user = context.RequestHeaders.GetValue("sessionid");
            var settings = request.Request;
            StorageOfAlgorithms.SendNewConfig(user, settings);
            return Task.FromResult(new UpdateServerConfigResponse());
        }
    }
}
