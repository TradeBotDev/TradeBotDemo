using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Former.FormerService.v1;
using static TradeBot.Former.FormerService.v1.FormerService;

namespace Algorithm.DataManipulation
{
    public static class PriceSender
    {
        private static GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:5003");
        private static FormerServiceClient client = new FormerServiceClient(channel);
        //private static SendPurchasePriceResponse call;
        

        public static void SendPrice (double price)
        {
            var response = client.SendPurchasePrice(new SendPurchasePriceRequest() { PurchasePrice = price });
        }


}
}
