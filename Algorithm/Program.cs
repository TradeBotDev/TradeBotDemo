using Algorithm.Analysis;
using Algorithm.DataManipulation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Algorithm
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }


       public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureServices(services => {
                    services.AddHostedService<Worker>();
                });
    }

    public class Worker : BackgroundService
    {

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await PriceSender.SendPrice();
            Publisher publisher = new();
            DataCollector dc = new();
            var pm = new PointMaker();
            AlgorithmAlpha algo = new();
            pm.Launch(publisher, dc);
        }
    }
}
