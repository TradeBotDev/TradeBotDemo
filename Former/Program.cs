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

            TradeMarketClient.Configure("https://localhost:5005", 10000, meta);
            TradeMarketClient observers = TradeMarketClient.GetInstance();
            Former.config = new Config
            {
                AvaibleBalance = 1.0,
                ContractValue = 10.0,
                RequiredProfit = 0.5,
                OrderUpdatePriceRange = 1.0,
                SlotFee = 0.2,
                TotalBalance = 0
            };

            //while (Former.config is null) { }

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
