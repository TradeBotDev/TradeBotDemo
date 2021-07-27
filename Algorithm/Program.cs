using Algorithm.Analysis;
using Algorithm.DataManipulation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace Algorithm
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var configuration = new ConfigurationBuilder()
                .Build();

            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            Log.Information("Algorithm started");

            CreateHostBuilder(args).Build().Run();
        }


       public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
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
            DataCollector dc = new(publisher);
            var pm = new PointMaker();
            AlgorithmAlpha algo = new(publisher);
            pm.Launch(publisher, dc);
        }
    }
}
