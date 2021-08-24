using System;
using System.Runtime.InteropServices;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using Prometheus;
using Serilog;

namespace AccountGRPC
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// Создание и запуск сервера, если операционной системой не является Windows.
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				int.TryParse(Environment.GetEnvironmentVariable("METRICS_PORT") ?? "6000", out int port);
				string host = Environment.GetEnvironmentVariable("METRICS_HOST") ?? "*";
				var server = new MetricServer(hostname: host, port: port);
				server.Start();
			}
			CreateHostBuilder(args).Build().Run();
		}

		// Additional configuration is required to successfully run gRPC on macOS.
		// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseSerilog()
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}