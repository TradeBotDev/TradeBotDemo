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
            var result = JsonConvert.DeserializeObject<List<Metadata>>(cache.StringGet(serviceId));
            result.Add(meta);
            cache.StringSet(serviceId, JsonConvert.SerializeObject(result));
        }
        public static void UpdateRecordInRedis(Metadata meta, AlgorithmInfo settings)
        {
            cache.StringSet(meta.GetValue("sessionid") + meta.GetValue("trademarket") + meta.GetValue("slot") + serviceId, JsonConvert.SerializeObject(settings));
        }
        public static void DeleteRecordFromRedis(Metadata meta)
        {
            cache.KeyDelete(meta.GetValue("sessionid") + meta.GetValue("trademarket") + meta.GetValue("slot") + serviceId.ToString());
            var result = JsonConvert.DeserializeObject<List<Metadata>>(cache.StringGet(serviceId));
            int toDelete = -1;
            foreach (var metadata in result)
            {
                if (metadata.GetValue("sessionid") == meta.GetValue("sessionid")
                    && metadata.GetValue("slot") == meta.GetValue("slot")
                    && metadata.GetValue("trademarket") == meta.GetValue("trademarket"))
                {
                    toDelete = result.IndexOf(metadata);
                }
            }
            if (toDelete != -1)
            {
                result.RemoveAt(toDelete);
            }
            cache.StringSet(serviceId, JsonConvert.SerializeObject(result));
        }
    }
}