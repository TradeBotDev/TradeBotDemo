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
            //commented out for testing purposes
            //DataCollector.SendPurchasePrice();
            //CreateHostBuilder(args).Build().Run();

            Publisher publisher = new();

            PointMaker PM = new PointMaker();
            PM.Launch(publisher);
              
        }


       /* public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureServices(services => {
                    services.AddHostedService<Worker>();
                });*/
    }

    /*public class Worker : BackgroundService
    {

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DataCollector.SendPurchasePrice();
        }
    }*/
}
