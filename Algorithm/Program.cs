using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Algorithm
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //commented out for testing purposes
            //DataCollector.SendPurchasePrice();
            //CreateHostBuilder(args).Build().Run();

            for (int i = 0; i<10; i++)
            {

            }
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
