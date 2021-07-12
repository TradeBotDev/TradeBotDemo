using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TradeBot.Common;
using TradeMarket.Former;

namespace Former
{
    public class AlgorithmAnswerService : Algorithm.AlgorithmAnswerService.AlgorithmAnswerServiceClient
    {
        private readonly ILogger<AlgorithmAnswerService> _logger;
        public AlgorithmAnswerService(ILogger<AlgorithmAnswerService> logger)
        {
            _logger = logger;
        }
        
        static async Task SubscribePurchasePrice()
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new OrderFormerService.OrderFormerServiceClient(channel);
            var orderSignature = new TradeBot.Common.OrderSignature
            {
                Status = OrderStatus.Open,
                Type = OrderType.Buy
            };
            var request = new SubscribeOrdersRequest()
            {
                Signature = orderSignature
            };
            using var call = client.SubscribeOrders(request);

            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
            }

        }
    }
}
