using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace History.Databases.Slot_History
{
    public class RedisClient
    {
        private readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING"));

        public RedisClient()
        {
            ISubscriber sub = redis.GetSubscriber();
            sub.Subscribe("Bitmex_Book25", (channel, message) => {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
            });
        }   
    }
}
