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

			// Получение из переменной окружения адреса Facade.
			string connectionString = Environment.GetEnvironmentVariable("FACADE_CONNECTION_STRING");

			// Если адрес Facade не был найден, происходит его получение из файла appsettings.json.
			if (connectionString == null)
			{
				// Создание конфигурации, которая содержит в себе все настройки из файла appsettings.json.
				var configuration = new ConfigurationBuilder()
					.AddJsonFile("appsettings.json", optional: false)
					.Build();
				// Получение адреса Facade из appsettings.json и возврат результата.
				return GrpcChannel.ForAddress(configuration.GetSection("GrpcClients")["FacadeService"]);
			}
			// Иначе используется он.
			else return GrpcChannel.ForAddress(connectionString);
		}
	}
}