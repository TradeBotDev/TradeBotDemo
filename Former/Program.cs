using Grpc.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using Serilog;
using TradeBot.Common.v1;

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
            var meta = new Metadata
            {
                { "sessionId", "123" },
                { "slot", "XBTUSD" }
            };

            //TradeMarketClient.Configure("https://localhost:5005", 10000, null);
            //TradeMarketClient observers = TradeMarketClient.GetInstance();

            //while (TradeMarketClient._entries is null) { }

            //observers.ObserveOrderBook();
            //observers.ObserveBalance();
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
