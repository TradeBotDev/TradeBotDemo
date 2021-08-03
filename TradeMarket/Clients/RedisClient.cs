using Newtonsoft.Json;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Clients
{
    public class RedisClient
    {
        private readonly IDatabase _db;

        public RedisClient(IConnectionMultiplexer multiplexer)
        {
            _db = multiplexer.GetDatabase();
        }

        public async Task Send<T>(string id, T data, string PublishingTopic)
        {
            Log.Information("{ServiceName} writing to Redis...", "Trademarket");
            string json = JsonConvert.SerializeObject(data);
            Task[] tasks = {
                _db.SetAddAsync(id, json),
                _db.PublishAsync(PublishingTopic,id)
            };
            await Task.WhenAll(tasks);
        }




    }
}

