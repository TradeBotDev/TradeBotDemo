using Grpc.Net.Client;

namespace Former
{
    public class Channels
    {
        public static GrpcChannel AlgorithmChannel = GrpcChannel.ForAddress("https://localhost:5001");
        public static GrpcChannel TradeMarketChannel = GrpcChannel.ForAddress("https://localhost:5005");
    }
}
