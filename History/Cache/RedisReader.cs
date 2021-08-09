using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace History.Cache
{
    public class RedisReader
    {
        private readonly ConnectionMultiplexer connectionMultiplexer;
        private readonly IDatabase db;
        public RedisReader()
        {
            connectionMultiplexer = ConnectionMultiplexer.Connect("localhost: 6379");
            db = connectionMultiplexer.GetDatabase();
        }

        public void ShowKeys()
        {
            EndPoint endPoint = connectionMultiplexer.GetEndPoints().First();
            RedisKey[] keys = connectionMultiplexer.GetServer(endPoint).Keys(pattern: "*").ToArray();

            foreach (var key in keys)
            {
                Console.WriteLine(key);
            }
        }
    }
}