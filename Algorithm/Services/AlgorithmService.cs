﻿using Grpc.Core;
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
        private static IServerStreamWriter<SubscribePurchasePriceResponse> streamWriter;
        public override async Task SubscribePurchasePrice(SubscribePurchasePriceRequest request, IServerStreamWriter<SubscribePurchasePriceResponse> sw, ServerCallContext context)
        {
            streamWriter = sw;
            AlgorithmEmulator algo = new AlgorithmEmulator();
            Random rnd = new Random();
            
            while (true) 
            {
                Thread.Sleep(rnd.Next(0, 10000));
                await streamWriter.WriteAsync(new SubscribePurchasePriceResponse { PurchasePrice = algo.CalculateSuggestedPrice() });
                Console.WriteLine("Sent something");
            }

        }
        public override async Task<AddOrderResponse> AddOrder(IAsyncStreamReader<AddOrderRequest> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var order = requestStream.Current;
                DataCollector.orders.Add(order.Order);
                Console.WriteLine("Got something");
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
