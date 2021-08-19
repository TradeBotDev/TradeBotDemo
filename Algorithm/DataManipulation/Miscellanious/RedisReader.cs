using Grpc.Core;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Algorithm.DataManipulation
{
    public static class RedisReader
    {
        private readonly static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING"));
        private readonly static IDatabase cache = redis.GetDatabase();
        private readonly static string serviceId = "5006";

        public static void SendNewRecordToRedis(Metadata meta, AlgorithmInfo settings)
        {
            cache.StringSet(meta.GetValue("sessionid") + meta.GetValue("trademarket") + meta.GetValue("slot") + serviceId, JsonConvert.SerializeObject(settings));
            var result = JsonConvert.DeserializeObject<string[]>(cache.StringGet(serviceId));
            var newResult = result.ToList();
            newResult.Add(JsonConvert.SerializeObject(meta));
            cache.StringSet(serviceId, JsonConvert.SerializeObject(newResult.ToArray()));
        }
        public static void UpdateRecordInRedis(Metadata meta, AlgorithmInfo settings)
        {
            cache.StringSet(meta.GetValue("sessionid") + meta.GetValue("trademarket") + meta.GetValue("slot") + serviceId, JsonConvert.SerializeObject(settings));
        }
        public static void DeleteRecordFromRedis(Metadata meta)
        {
            cache.KeyDelete(meta.GetValue("sessionid") + meta.GetValue("trademarket") + meta.GetValue("slot") + serviceId.ToString());
        }
    }
}