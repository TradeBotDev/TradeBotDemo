using Algorithm.Analysis;
using Grpc.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Algorithm.DataManipulation
{
    public static class StorageOfAlgorithms
    {
        private static Dictionary<Metadata, AlgorithmBeta> algorithms = new();
        private static OrderPublisher orderPublisher = new();
        private static List<Thread> threadsWithAlgos = new();

        public static void SendNewOrderToAllAlgos(Order order, Metadata metadata)
        {
            orderPublisher.Publish(order, metadata);
        }
        public static void SendNewConfig (Metadata metadata, UpdateServerConfigRequest configRequest)
        {
            if (!MetaExists(metadata))
            {
                threadsWithAlgos.Add(new Thread(()=>CreateAlgorithm(configRequest.Config.AlgorithmInfo, metadata)));
                threadsWithAlgos.Last().Start();
                return;
            }

            if (GetAlgoByMeta(metadata).GetState()!=configRequest.Switch)
            {
                algorithms[metadata].ChangeState();
                return;
            }

            GetAlgoByMeta(metadata).ChangeSetting(configRequest.Config.AlgorithmInfo);
        }
        private static void CreateAlgorithm(AlgorithmInfo setting, Metadata metadata)
        {
            Log.Information("{@Where}: Initiated algorithm creation for user {@User}", "Algorithm", metadata.GetValue("sessionid"));
            bool result = algorithms.TryAdd(metadata, new AlgorithmBeta(metadata));
            if (result)
            {    
                orderPublisher.OrderIncomingEvent += algorithms[metadata].NewOrderAlert;
                algorithms[metadata].ChangeSetting(setting);
                algorithms[metadata].ChangeState();
            }
        }

        private static bool MetaExists(Metadata metadata)
        {
            foreach (KeyValuePair<Metadata, AlgorithmBeta> existingAlgo in algorithms)
            {
                if (metadata.GetValue("sessionid") == existingAlgo.Key.GetValue("sessionid")
                    && metadata.GetValue("slot") == existingAlgo.Key.GetValue("slot")
                    && metadata.GetValue("trademarket") == existingAlgo.Key.GetValue("trademarket")
                    && metadata.GetValue("user-agent") == existingAlgo.Key.GetValue("user-agent")
                    && metadata.GetValue("traceparent") == existingAlgo.Key.GetValue("traceparent"))
                {
                    return true;
                }
            }
            return false;
        }

        private static AlgorithmBeta GetAlgoByMeta(Metadata metadata)
        {
            foreach (KeyValuePair<Metadata, AlgorithmBeta> existingAlgo in algorithms)
            {
                if (metadata.GetValue("sessionid") == existingAlgo.Key.GetValue("sessionid")
                    && metadata.GetValue("slot") == existingAlgo.Key.GetValue("slot")
                    && metadata.GetValue("trademarket") == existingAlgo.Key.GetValue("trademarket")
                    && metadata.GetValue("user-agent") == existingAlgo.Key.GetValue("user-agent")
                    && metadata.GetValue("traceparent") == existingAlgo.Key.GetValue("traceparent"))
                {
                    return existingAlgo.Value;
                }
            }
            throw new Exception("Algo not found");
        }
    }
}