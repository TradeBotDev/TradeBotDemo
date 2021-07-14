using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Algorithm.Relay;

namespace Algorithm
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

            var relayAddress = GrpcChannel.ForAddress("https://localhost:5004");
            var client = new SendToAlgorithm.SendToAlgorithmClient(relayAddress);
            var reply = client.GetOrders(new GetOrdersRequest { });

        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
