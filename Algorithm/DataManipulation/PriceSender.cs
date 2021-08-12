using Grpc.Net.Client;
using Serilog;
using System;
using TradeBot.Former.FormerService.v1;

using static TradeBot.Former.FormerService.v1.FormerService;

namespace Algorithm.DataManipulation
{
    //sends the price to Former
    //all the values are hardcoded for now 
    public static class PriceSender
    {
        private static readonly GrpcChannel Channel = GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("FORMER_CONNECTION_STRING"));
        private static readonly FormerServiceClient Client = new FormerServiceClient(Channel);
        public static void SendDecision (int decision, string user)
        {
            var response = Client.SendAlgorithmDecision(new SendAlgorithmDecisionRequest() { Decision = decision }, StorageOfAlgorithms.GetMetaByUser(user));
            Log.Information("Sent " + decision + "  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }
    }
}
