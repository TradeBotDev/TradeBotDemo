using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Former.Models;
using StackExchange.Redis;

namespace Former.Clients
{
    public static class RedisClient
    {
        private static readonly ConnectionMultiplexer Redis = ConnectionMultiplexer.Connect(new ConfigurationOptions{ EndPoints = {Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")} });
        private static readonly IDatabase Database= Redis.GetDatabase();
        private const string ServiceId = "5003";


        public static async Task WriteMeta(List<Metadata> metadata)
        {
            var value = JsonSerializer.Serialize(metadata);

            if (await Database.KeyExistsAsync(ServiceId))
            {
                var oldValue = Database.StringGet(ServiceId);
                await Database.SetRemoveAsync(ServiceId, oldValue);
                await Database.SetAddAsync(ServiceId, value);
            }
            else await Database.SetAddAsync(ServiceId, value);
        }

        public static async Task<List<Metadata>> ReadMeta()
        {
            if (await Database.KeyExistsAsync(ServiceId))
            {
                return JsonSerializer.Deserialize<List<Metadata>>(await Database.StringGetAsync(ServiceId));
            }
            return null;
        }

        public static async Task WriteConfiguration(Metadata metadata, Configuration configuration)
        {
            var key = metadata.Sessionid + metadata.Trademarket + metadata.Slot + ServiceId;
            var value = JsonSerializer.Serialize(configuration);

            if (await Database.KeyExistsAsync(key))
            {
                var oldValue = Database.StringGet(key);
                await Database.SetRemoveAsync(key, oldValue);
                await Database.SetAddAsync(key, value);
            }
            else await Database.SetAddAsync(key, value);
        }

        public static async Task<Configuration> ReadConfiguration(Metadata metadata)
        {
            var key = metadata.Sessionid + metadata.Trademarket + metadata.Slot + ServiceId;
            if (await Database.KeyExistsAsync(key))
            {
                return JsonSerializer.Deserialize<Configuration>(await Database.StringGetAsync(key));
            }
            return null;
        }

        public static async Task DeleteMetaEntries()
        {
            while (await Database.KeyExistsAsync(ServiceId))
            {
                var oldValue = Database.StringGet(ServiceId);
                await Database.SetRemoveAsync(ServiceId, oldValue);
            }
        }

        public static async Task DeleteConfigurations(List<Metadata> metadata)
        {
            foreach (var meta in metadata)
            {
                var key = meta.Sessionid + meta.Trademarket + meta.Slot + ServiceId;
                var oldValue = Database.StringGet(key);
                await Database.SetRemoveAsync(key, oldValue);
            }
        }
    }
}
