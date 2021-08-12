using Algorithm.Analysis;
using Grpc.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Algorithm.DataManipulation
{
    public static class StorageOfAlgorithms
    {
        private static Dictionary<string, AlgorithmBeta> algorithms = new();
        private static OrderPublisher orderPublisher = new();
        private static Dictionary<string, Metadata> metaDict = new();

        public static Metadata GetMetaByUser(string user)
        {
            return metaDict[user];
        }
        public static void SendNewMeta(string user, Metadata meta)
        {
            metaDict.TryAdd(user, meta);
        }
        public static void SendNewOrderToAllAlgos(Order order)
        {
            orderPublisher.Publish(order);
        }
        public static void SendNewConfig (string user, UpdateServerConfigRequest configRequest)
        {
            Log.Information("GOT NEW CONFIG HERE, I WANT TO MAKE AN ALGO");
            if (!algorithms.ContainsKey(user))
            {
                Log.Information("IM ABOUT TO CREATE AN ALGOOOOOOO");
                CreateAlgorithm(configRequest.Config.AlgorithmInfo, user);
                return;
            }

            if (algorithms[user].GetState()!=configRequest.Switch)
            {
                algorithms[user].ChangeState();
                return;
            }

            algorithms[user].ChangeSetting(configRequest.Config.AlgorithmInfo);
        }
        private static void CreateAlgorithm(AlgorithmInfo setting, string user)
        {
            Log.Information("IM LITERALLY CREATING A NEW ALGO RIGHT NOW");
            bool result = algorithms.TryAdd(user, new AlgorithmBeta(user));
            if (result)
            {
                Log.Information("{@Where}: Created a new algorithm for a user {@User}", "Algorithm", user);
                Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                orderPublisher.OrderIncomingEvent += algorithms[user].NewOrderAlert;
                algorithms[user].ChangeState();
            }
        }
    }
}
