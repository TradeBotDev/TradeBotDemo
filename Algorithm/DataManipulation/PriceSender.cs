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
        private static readonly GrpcChannel Channel = GrpcChannel.ForAddress("https://localhost:5003");
        private static readonly FormerServiceClient Client = new FormerServiceClient(Channel);
        public static void SendPrice (double price)
        {
            var response = Client.SendPurchasePrice(new SendPurchasePriceRequest { PurchasePrice = price }, DataCollector.metaData);
            Log.Information("Sent " + price + "  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }
    }
}
