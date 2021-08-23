using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;

namespace Website.Controllers.Clients
{
	// Класс с методами для подключения к сервису AccountService.
	public static class AccountServiceConnection
	{
		// Метод, который подключается к сервису и возвращает подключение. 
		public static GrpcChannel GetConnection()
		{
			Log.Information($"AccountServiceConnection: вызван метод GetConnection.");

			string connectionString = Environment.GetEnvironmentVariable("FACADE_CONNECTION_STRING");
			if (connectionString == null)
			{
				// Создание конфигурации, которая содержит в себе все настройки из файла appsettings.json.
				var configuration = new ConfigurationBuilder()
					.AddJsonFile("appsettings.json", optional: false)
					.Build();
				return GrpcChannel.ForAddress(configuration.GetSection("GrpcClients")["FacadeService"]);
			}
			else return GrpcChannel.ForAddress(connectionString);
		}
	}
}