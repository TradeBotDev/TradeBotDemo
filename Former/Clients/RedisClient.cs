using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Former.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Former.Clients
{
    public static class RedisClient
    {
        private static readonly ConnectionMultiplexer Redis = ConnectionMultiplexer.Connect(new ConfigurationOptions{ EndPoints = {Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")} });
        private static readonly IDatabase Database= Redis.GetDatabase();
        private const string ServiceId = "5003";


        public static async Task WriteMetaEntries(List<Metadata> metadata)
        {
            var value = JsonConvert.SerializeObject(metadata);

            if (await Database.KeyExistsAsync(ServiceId))
            {
                await Database.KeyDeleteAsync(ServiceId);
                await Database.StringSetAsync(ServiceId, value);
            }
            else await Database.StringSetAsync(ServiceId, value);
        }

        public static async Task<List<Metadata>> ReadMeta()
        {
            if (await Database.KeyExistsAsync(ServiceId))
            {
                return JsonConvert.DeserializeObject<List<Metadata>>(await Database.StringGetAsync(ServiceId));
            }
            return null;
        }

        public static async Task WriteConfiguration(Metadata metadata, Configuration configuration)
        {
            var key = metadata.Sessionid + metadata.Trademarket + metadata.Slot + ServiceId;
            var value = JsonConvert.SerializeObject(configuration);

            if (await Database.KeyExistsAsync(key))
            {
                await Database.KeyDeleteAsync(key);
                await Database.StringSetAsync(key, value);
            }
            else await Database.StringSetAsync(key, value);
        }

        public static async Task<Configuration> ReadConfiguration(Metadata metadata)
        {
            var key = metadata.Sessionid + metadata.Trademarket + metadata.Slot + ServiceId;
            if (await Database.KeyExistsAsync(key))
            {
                return JsonConvert.DeserializeObject<Configuration>(await Database.StringGetAsync(key));
            }
            return null;
        }

        public static async Task DeleteMetaEntries()
        {
            while (await Database.KeyExistsAsync(ServiceId))
            {
                await Database.KeyDeleteAsync(ServiceId);
            }
        }

        public static async Task DeleteConfigurations(List<Metadata> metadata)
        {
            foreach (var meta in metadata)
            {
                var key = meta.Sessionid + meta.Trademarket + meta.Slot + ServiceId;
                await Database.KeyDeleteAsync(key);
            }
        }
    }
}
