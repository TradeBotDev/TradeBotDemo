using System;
using System.Runtime.InteropServices;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using Prometheus;
using Serilog;

namespace Website
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// Создание и запуск сервера, если операционной системой не является Windows.
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				int.TryParse(Environment.GetEnvironmentVariable("METRICS_PORT") ?? "6008", out int port);
				string host = Environment.GetEnvironmentVariable("METRICS_HOST") ?? "*";
				var server = new MetricServer(hostname: host, port: port);
				server.Start();
			}
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseSerilog()
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}