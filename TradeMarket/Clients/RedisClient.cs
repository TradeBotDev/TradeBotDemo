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
            _db = multiplexer == null ? null : multiplexer.GetDatabase();        
        }

        public async Task Send<T>(string id, T data, string PublishingTopic)
        {
            if (_db is not null)
            {
                string json = JsonConvert.SerializeObject(data);
                Log.Information("{@ServiceName} writing to Redis {@Data}", "Trademarket", json);

                Task[] tasks = {
                _db.SetAddAsync(id, json),
                _db.PublishAsync(PublishingTopic,id)
                };
                await Task.WhenAll(tasks);
            }
        }




    }
}

