using System;
using System.Collections.Generic;
using Grpc.Core;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace Former.Clients
{
    public class RedisClient
    {
        private readonly IDatabase _database;
        private const string ServiceId = "5003";

        public RedisClient()
        {
            var redis = ConnectionMultiplexer.Connect(new ConfigurationOptions{ EndPoints = {Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")} });
            _database = redis.GetDatabase();
        }

        public void WriteContext(List<Metadata> contexts)
        {
            var json = 


        }

        public bool ReadContext()
        {

        }


    }
}
