using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;

namespace Website.Controllers.Clients
{
    public static class AccountServiceConnection
    {
        public static GrpcChannel GetConnection()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            bool useDocker = Convert.ToBoolean(configuration.GetSection("GrpcClients")["UseDocker"]);
            if (useDocker)
                return GrpcChannel.ForAddress(configuration.GetSection("GrpcClients")["AccountServiceDocker"]);
            else return GrpcChannel.ForAddress(configuration.GetSection("GrpcClients")["AccountServiceLocal"]);
        }
    }
}
