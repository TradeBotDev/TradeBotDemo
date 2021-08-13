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
            if (!algorithms.ContainsKey(metadata))
            {
                threadsWithAlgos.Add(new Thread(()=>CreateAlgorithm(configRequest.Config.AlgorithmInfo, metadata)));
                threadsWithAlgos.Last().Start();
                return;
            }

            if (algorithms[metadata].GetState()!=configRequest.Switch)
            {
                algorithms[metadata].ChangeState();
                return;
            }

            algorithms[metadata].ChangeSetting(configRequest.Config.AlgorithmInfo);
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
    }
}