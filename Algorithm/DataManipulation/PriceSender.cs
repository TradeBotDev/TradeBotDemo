using Grpc.Net.Client;

using TradeBot.Former.FormerService.v1;

using static TradeBot.Former.FormerService.v1.FormerService;

namespace Algorithm.DataManipulation
{
    public static class PriceSender
    {
        private static readonly GrpcChannel Channel = GrpcChannel.ForAddress("https://localhost:5003");
        private static readonly FormerServiceClient Client = new FormerServiceClient(Channel);
        //private static SendPurchasePriceResponse call;
        

        public static void SendPrice (double price)
        {
            var response = Client.SendPurchasePrice(new SendPurchasePriceRequest { PurchasePrice = price });
        }


}
}
