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
        //UNCOMMENT FOR TESTING
        //private static readonly GrpcChannel Channel = GrpcChannel.ForAddress("https://localhost:5042");
        //private static readonly TestingServiceClient Client = new TestingServiceClient(Channel);

        //UNCOMMENT FOR WORKING VERSION
        private static readonly GrpcChannel Channel = GrpcChannel.ForAddress("https://localhost:5003");
        private static readonly FormerServiceClient Client = new FormerServiceClient(Channel);
        public static void SendPrice (int decision)
        {
            var response = Client.SendAlgorithmDecision(new SendAlgorithmDecisionRequest() { Decision = decision }, DataCollector.metaData);
            Log.Information("Sent " + decision + "  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }
    }
}
