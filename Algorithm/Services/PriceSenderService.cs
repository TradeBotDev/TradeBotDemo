using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common;
using System.Threading;
using TradeBot.Algorithm.PriceService.v1;

namespace Algorithm.Services
{

    public class PriceSenderService : PriceService.PriceServiceBase
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
            }

        }



    }
}
