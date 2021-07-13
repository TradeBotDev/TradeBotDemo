using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common;

namespace Algorithm.Services
{
    public class PriceSenderService : AlgorithmAnswerService.AlgorithmAnswerServiceBase
    {

        public override Task<SubscribePurchasePriceReply> SubscribePurchasePrice(SubscribePurchasePriceRequest request, IServerStreamWriter<SubscribePurchasePriceReply> sw,ServerCallContext context)
        {
            AlgorithmEmulator algo = new AlgorithmEmulator();
            return Task.FromResult(new SubscribePurchasePriceReply { PurchasePrice = algo.CalculateSuggestedPrice(new List<Order> { }, new TradeBot.Common.AlgorithmInfo()) });
        }

    }
}
