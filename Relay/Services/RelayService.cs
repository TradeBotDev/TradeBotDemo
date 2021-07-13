using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Relay.Facade;
using TradeBot.Algorithm.Relay;
using TradeBot.Common;

namespace Relay
{
    class RelayService : SendToRelay.SendToRelayBase
    {
        private readonly ILogger<RelayService> _logger;
        public RelayService(ILogger<RelayService> logger) => _logger = logger;

        public override Task<StartBotReply> StartBot(StartBotRequest request, ServerCallContext context)
        {
            Random rnd = new Random(); //пока так

            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new SendToAlgorithm.SendToAlgorithmClient(channel);

            var getOrdersRequest = new GetOrdersRequest();

            var configRequest = new GetConfigRequest
            {
                Config = new Config
                {
                    TotalBalance = rnd.Next(100, 1000),
                    AvaibleBalance = rnd.Next(25, 100),
                    RequiredProfit = 25,
                    SlotFee = 0.25,
                    OrderUpdatePriceRange = 15,
                    AlgorithmInfo = new AlgorithmInfo
                    {
                        Interval = new Google.Protobuf.WellKnownTypes.Timestamp(),
                        PointSplitCount = 1000
                    }
                }
            };

            client.GetOrders(); //пустышка!
            client.GetConfigAsync(configRequest, null);

            DefaultReply reply = new DefaultReply
            {
                Code = ReplyCode.Succeed,
                Message = "Бот запущен успешно"
            };

            StartBotReply result = new StartBotReply
            {
                Reply = reply
            };

            return Task.FromResult<StartBotReply>(result);
        }

        public override async Task<SubscribeLogsReply> SubscribeLogs(IAsyncStreamReader<SubscribeLogsRequest> request, ServerCallContext context)
        {
            while (await request.MoveNext())
            {
                //пока ничего
            }

            DefaultReply reply = new DefaultReply
            {
                Code = ReplyCode.Succeed,
                Message = "Подписка на логи совершена"
            };

            return new SubscribeLogsReply { Reply = reply };
        }
    }
}
