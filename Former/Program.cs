using Grpc.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace Former
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            Metadata meta = new Metadata();
            meta.Add("sessionId","123");
            meta.Add("slot", "XBTUSD");

            TradeMarketClient.Configure("https://localhost:5005", 10000, meta);
            TradeMarketClient observers = TradeMarketClient.GetInstance();



            observers.ObserveOrderBook();
            observers.ObserveBalance();
            //observers.ObserveMyOrders();

            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}
