using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket;
using TradeBot.Common;

namespace Former
{
    class BuyOrder 
    {
        string id;
        Order simple_order_info;
        DateTime RemoveDate;
        double RemovePrice;
    }
    class Order 
    {
        double Quantity;
        double Price;
        OrderSignature Signature;
        DateTime LastUpdateDate;
    }
    class OrderSignature
    {
        double quantity = 1;
        double price = 2;
        OrderSignature signature = 3;
        google.protobuf.Timestamp last_update_date = 4;
    }


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
            var orderSignature = new OrderSignature
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
                response.
            }

        }
    }
}
