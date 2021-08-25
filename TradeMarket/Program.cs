using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TradeMarket.DataTransfering;

namespace TradeMarket
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Verbose()
               .WriteTo.Console()
               .Enrich.WithEnvironmentName()
               .Enrich.WithMemoryUsage()
               .Enrich.WithThreadName()
               .Enrich.WithThreadId()
               .Enrich.FromLogContext();
            //если дебаг, а он на винде, то сек должен быть внутри системы запущен. если в докере то там развернут сек
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                loggerConfiguration.WriteTo.Seq("http://localhost:5341");
            }
            else
            {
                loggerConfiguration.WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_CONNECTION_STRING"));
                var server = new MetricServer(hostname: "*", port: 6005);
                server.Start();
            }

            Log.Logger = loggerConfiguration.CreateLogger();
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureServices(services => {
                    services.AddHostedService<Worker>();
                });
    }

}
