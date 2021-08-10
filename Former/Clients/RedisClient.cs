using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Former.Clients
{
    public class RedisClient
    {
        private const string RedisConnectionString = "localhost:6379";
        private static ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(RedisConnectionString);

  
        static void Lol()
        {
            var pubSubConnection = connection.GetSubscriber();
  
            //pubSubConnection.Subscribe("cyka", (channel, message) => )
        }
    }
}
