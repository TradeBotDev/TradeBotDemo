using History.DataBase;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using TradeBot.Common.v1;

namespace History
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //DataContext postgres = new();
            //Console.ReadKey();
            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.Debug()
            //    .WriteTo.Console()
            //    .WriteTo.Seq("http://localhost:5341")
            //    .CreateLogger();
            //CreateHostBuilder(args).Build().Run();

            DataContext db = new();
            //db.Add(new BalanceChange { Balance = new Balance(), SessionId = "meow", Time = DateTime.Now });
            Console.WriteLine("did it");
            Console.ReadKey();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
