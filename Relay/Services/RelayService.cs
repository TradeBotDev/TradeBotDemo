using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common;
using TradeBot.Common.v1;
using TradeBot.Relay.RelayService.v1;
using TradeBot.Algorithm.AlgorithmService.v1;
using TradeBot.Former.FormerService.v1;
using System.Collections.ObjectModel;
using System.Collections;
using TradeBot.TradeMarket.TradeMarketService.v1;

namespace Relay
{
    class Relay : RelayService.RelayServiceBase
    {
        private readonly ILogger<Relay> _logger;
        public Relay(ILogger<Relay> logger) => _logger = logger;

        private ObservableCollection<Order> orders = new ObservableCollection<Order>();

        private bool IsStarted = true;
        private Config config;

        //ДОПИСАН
        public override Task<StartBotResponse> StartBot(StartBotRequest request, ServerCallContext context)
        {
            StartBotResponse result = new StartBotResponse();
            IsStarted = !IsStarted;
            result.Response.Code = ReplyCode.Succeed;

            if (IsStarted) result.Response.Message = "Бот успешно запущен";
            else result.Response.Message = "Бот выключен";
            Console.WriteLine(result.Response.Message);

            return Task.FromResult<StartBotResponse>(result);
        }

        //ДОПИСАН
        public override Task<TradeBot.Relay.RelayService.v1.UpdateServerConfigResponse> UpdateServerConfig(TradeBot.Relay.RelayService.v1.UpdateServerConfigRequest request,
            ServerCallContext context)
        {
            this.config = request.Request.Config;
            var response = new TradeBot.Relay.RelayService.v1.UpdateServerConfigResponse();
            Console.WriteLine("Конфиг в Relay был обновлен");
            return Task.FromResult(response);
        }

        public override Task SubscribeLogs(TradeBot.Relay.RelayService.v1.SubscribeLogsRequest request,
            IServerStreamWriter<TradeBot.Relay.RelayService.v1.SubscribeLogsResponse> responseStream,
            ServerCallContext context)
        {
            return base.SubscribeLogs(request, responseStream, context);
        }

        //ДОПИСАН
        public override async Task<TradeBot.Relay.RelayService.v1.AddOrderResponse> AddOrder(IAsyncStreamReader<TradeBot.Relay.RelayService.v1.AddOrderRequest> requestStream,
            ServerCallContext context)
        {
            GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new AlgorithmService.AlgorithmServiceClient(channel);
            var stream = client.AddOrder();

            orders.CollectionChanged += async (source, args) =>
            {
                await writeToStreamAsync(stream, args.NewItems);
            };

            while (await requestStream.MoveNext())
            {
                var request = new TradeBot.Algorithm.AlgorithmService.v1.AddOrderRequest();
                request.Order = requestStream.Current.Order;
                await stream.RequestStream.WriteAsync(request);
                Console.WriteLine($"Товар {request.Order.Id} отправлен алгоритму");
            }
            //await stream.RequestStream.CompleteAsync();

            var response = new TradeBot.Relay.RelayService.v1.AddOrderResponse();
            return response;
        }

        private async Task writeToStreamAsync(AsyncClientStreamingCall<TradeBot.Algorithm.AlgorithmService.v1.AddOrderRequest, TradeBot.Algorithm.AlgorithmService.v1.AddOrderResponse> stream, IList newItems)
        {
            //TODO тут вот пока синхронно
            foreach (var order in newItems)
            {
                await stream.RequestStream.WriteAsync(new TradeBot.Algorithm.AlgorithmService.v1.AddOrderRequest
                {
                    Order = order as Order
                }); 
            }
        }

        public async override Task SubscribeOrders(TradeBot.Relay.RelayService.v1.SubscribeOrdersRequest request, IServerStreamWriter<TradeBot.Relay.RelayService.v1.SubscribeOrdersResponse> responseStream, ServerCallContext context)
        {
            GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:5005");
            var trademarketclient = new TradeMarketService.TradeMarketServiceClient(channel);

            var orderSignature = new OrderSignature
            {
                Status = OrderStatus.Closed,
                //TODO пока только на продажу
                Type = OrderType.Sell
            };

            var orderRequest = new TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersRequest()
            {
                Request = new TradeBot.Common.v1.SubscribeOrdersRequest
                {
                    Signature = orderSignature
                }
            };

            using var call = trademarketclient.SubscribeOrders(orderRequest);
            while (await call.ResponseStream.MoveNext())
            {
                orders.Add(call.ResponseStream.Current.Response.Order);
                Console.WriteLine($"Получен товар {call.ResponseStream.Current.Response.Order.Id} из TradeMarket");
            }
        }
    }
}
