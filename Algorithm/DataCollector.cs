using Algorithm.Services;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.Former.FormerService.v1;

namespace Algorithm
{
    public class DataCollector
    {
        public List<Order> orders;
        public bool initialAnalysisCompleted;

        public static void SendPurchasePrice()
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5003");
            var client = new FormerService.FormerServiceClient(channel);
            SendPurchasePriceResponse call;
            AlgorithmEmulator algo = new AlgorithmEmulator();
            Random rnd = new Random();

            while (true)
            {
                Thread.Sleep(rnd.Next(0, 5000));
                double newPrice = algo.CalculateSuggestedPrice();
                call = await client.SendPurchasePriceAsync(new SendPurchasePriceRequest() { PurchasePrice = newPrice });
                Console.WriteLine("Sent " + newPrice);
            }

        }

    }
}
