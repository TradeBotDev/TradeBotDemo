using Algorithm.DataManipulation;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.Former.FormerService.v1;

namespace Algorithm
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DataCollector.SendPurchasePrice();
            CreateHostBuilder(args).Build().Run();
        }


       /* public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
