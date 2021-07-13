using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common;
using System.Threading;
using Algorithm.Former;

namespace Algorithm.Services
{

    public class PriceSenderService : AlgorithmObserverService.AlgorithmObserverServiceBase
    {
        private static IServerStreamWriter<SubscribePurchasePriceReply> streamWriter;
        public override async Task SubscribePurchasePrice(SubscribePurchasePriceRequest request, IServerStreamWriter<SubscribePurchasePriceReply> sw, ServerCallContext context)
        {
            streamWriter = sw;
            AlgorithmEmulator algo = new AlgorithmEmulator();
            Random rnd = new Random();
            
            while (true) 
            {
                Thread.Sleep(rnd.Next(0, 10000));
                await streamWriter.WriteAsync(new SubscribePurchasePriceReply { PurchasePrice = algo.CalculateSuggestedPrice() });
            }

        }



    }
}
