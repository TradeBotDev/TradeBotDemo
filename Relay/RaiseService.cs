using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StackExchange.Redis;
using TradeBot.Common.v1;


namespace Relay
{
    public class RaiseService
    {
        private readonly static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING"));
        private static IDatabase database = redis.GetDatabase();
        static string serviceId = "5004";
        public RaiseService()
        {
            if(database.KeyExists(serviceId))
            {
                //если в бд присутствуют данные релея => восстанавливаем все потоки  
                Clients.AlgorithmClient alg = new Clients.AlgorithmClient(Environment.GetEnvironmentVariable("ALGORITHM_CONNECTION_STRING"));
                Clients.FormerClient former = new Clients.FormerClient(Environment.GetEnvironmentVariable("FORMER_CONNECTION_STRING"));
                Clients.TradeMarketClient tm = new Clients.TradeMarketClient(Environment.GetEnvironmentVariable("TRADEMARKET_CONNECTION_STRING"));
                //получаем список мет и конфигов незавершенных сеансов  
                var list = Deserailization();
                for (int i = 0; i < list.Count; i++)
                {
                    Metadata mt = new Metadata
                    {
                        {"sessionid",list[i].list[0]},
                        {"slot", list[i].list[1]},
                        {"trademarket", list[i].list[2]}
                    };
                    //создаем экземпляр класса релей с соответствующими конфигами и метами
                    new Services.RelayService(alg, tm, former).RelayService2(alg, tm, former, mt, list[i].config);
                }
            }
        }
        /// <summary>
        /// При подключени пользователя его данные добавляются в редис
        /// </summary>
        /// <param name="config"></param>
        /// <param name="requestHeaders"></param>
        /// <param name="updateSwitch"></param>
        public static async void AddToRedis(Config config, Metadata requestHeaders,bool updateSwitch=false)
        {
            MetaAndConfig mac = new MetaAndConfig(requestHeaders, config);

            if (database.KeyExists(serviceId))
            {
                //получаем список мет и конфигов незавершенных сеансов 
                var list = Deserailization();
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].list[0] == mac.list[0])
                    {
                        //если данные пользователя присутствуют в бд => удаляем и записываем заново 
                        list.RemoveAt(i);
                    }
                }
                list.Add(mac);
            }
            else
            {
                //если в бд нет записей релея => добавляем
                List<MetaAndConfig> list = new List<MetaAndConfig>() { mac };
                await Serialization(list);
            }
        }
        /// <summary>
        /// Если пользователь вышел из системы => удаляем его данные из бд
        /// </summary>
        /// <param name="user"></param>
        public static async void DeleteFromRedis(Model.UserContext user)
        {
            if(database.KeyExists(serviceId))
            {
                List<MetaAndConfig> list = Deserailization();
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].list[0] == user.Meta.GetValue("sessionid"))
                    {
                        list.RemoveAt(i);
                    }
                }
                await Serialization(list);
                await database.KeyDeleteAsync(serviceId);
            }
        }
        //запись в бд
        private async static Task Serialization(List<MetaAndConfig> list)
        {
            string json = JsonConvert.SerializeObject(list, Formatting.Indented);
            await database.StringSetAsync(serviceId, json);
        }
        //чтение из бд
        private static List<MetaAndConfig> Deserailization()
        {
            var json = database.StringGet(serviceId);
            var list = JsonConvert.DeserializeObject<List<MetaAndConfig>>(json);
            return list;
        }

        private class MetaAndConfig
        {
            public List<string> list = null;
            public Config config = null;
            public MetaAndConfig() { }
            public MetaAndConfig(Metadata meta,Config config)
            {
                list = ConvertMetadata(meta);
                this.config = config;
            }
            private static List<string> ConvertMetadata(Metadata meta)
            {
                List<string> list = new List<string>();
                list.Add(meta.GetValue("sessionid"));
                list.Add(meta.GetValue("slot"));
                list.Add(meta.GetValue("trademarket"));
                return list;
            }
        }


    }
}
