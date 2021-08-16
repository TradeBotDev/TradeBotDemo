using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;

namespace Website.Controllers.Clients
{
    // Класс с методами для подключения к сервису AccountService.
    public static class AccountServiceConnection
    {
        // Метод, который подключается к сервису и возвращает подключение. 
        public static GrpcChannel GetConnection()
        {
            // Создание конфигурации, которая содержит в себе все настройки из файла appsettings.json.
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            // Конвертация параметра с переключением строки подключения (для докера или локально).
            bool useDocker = Convert.ToBoolean(configuration.GetSection("GrpcClients")["UseDocker"]);

            // Если выбран докер, возвращается строка подключения к сервису, который тоже находится в докере.
            if (useDocker)
                return GrpcChannel.ForAddress(configuration.GetSection("GrpcClients")["AccountServiceDocker"]);
            // Иначе используется localhost
            else return GrpcChannel.ForAddress(configuration.GetSection("GrpcClients")["AccountServiceLocal"]);
        }
    }
}
