using History.DataBase;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;

namespace History
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_CONNECTION_STRING"))
                .CreateLogger();

            using (var db = new DataContext())
            {
                Log.Information("{@Where}: Currently in History:", "History");
                Log.Information("{@Where}: Balance records:", "History");
                var balanceRecords = db.BalanceRecords.Include(x => x.Balance).ToList();
                if (balanceRecords.Count() != 0)
                {
                    foreach (var record in balanceRecords)
                    {
                        Log.Information("{@Where}: User: {@User}, new balance: {@Balance}, time of change: {@Time}", "History", record.UserId, record.Balance.Value.ToString() + record.Balance.Currency, record.Time);
                    }
                }
                Log.Information("{@Where}: Order records:", "History");
                var orderRecords = db.OrderRecords.Include(x => x.Order).ToList();
                if (orderRecords.Count() != 0)
                {
                    foreach (var record in orderRecords)
                    {
                        Log.Information("{@Where}: User: {@User}, order id: {@Order}, type of change: {@Time}", "History", record.UserId, record.Order.OrderIdOnTM, record.ChangesType);
                    }
                }
            }

                CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
