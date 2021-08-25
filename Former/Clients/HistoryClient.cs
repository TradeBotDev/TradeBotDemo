using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Former.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
using TradeBot.History.HistoryService.v1;
using Metadata = Grpc.Core.Metadata;

namespace Former.Clients
{
    public class HistoryClient
    {
        private static int _retryDelay;
        private static string _connectionString;
        private readonly ILogger _logger;

        private readonly HistoryService.HistoryServiceClient _client;

        public static void Configure(string connectionString, int retryDelay)
        {
            _connectionString = connectionString;
            _retryDelay = retryDelay;
        }

        public HistoryClient(ILogger logger)
        {
            _logger = logger.ForContext<HistoryClient>();
            _client = new HistoryService.HistoryServiceClient(GrpcChannel.ForAddress(_connectionString));
        }

        /// <summary>
        /// Проверяет соединение с биржей, на вход принимает функцию, осуществляющую общение с биржей
        /// </summary>
        private async Task ConnectionTester(Func<Task> func)
        {
            var attempts = 0;
            while (true)
            {
                try
                {
                    await func.Invoke();
                    break;
                }
                catch (RpcException e)
                {
                    if (e.StatusCode == StatusCode.Cancelled) break;
                    if (attempts > 3)
                    {
                        _logger.Error("Error {@ExceptionMessage}. Retrying...\r\n{@ExceptionStackTrace}", e.Message, e.StackTrace);
                        throw new RpcException(e.Status);
                    }
                    _logger.Error("Error {@ExceptionMessage}. Retrying...\r\n{@ExceptionStackTrace}", e.Message, e.StackTrace);
                    Thread.Sleep(_retryDelay);
                    attempts++;
                }
            }
        }

        /// <summary>
        /// Отправляет обновлённый баланс в сервис истории с указанием валюты и времени.
        /// </summary>
        internal async Task<PublishEventResponse> WriteBalance(double balance, Metadata meta)
        {
            PublishEventResponse response = null;

            async Task PublishBalanceUpdateFunc()
            {
                response = await _client.PublishEventAsync(new PublishEventRequest
                {
                    Balance = new PublishBalanceEvent
                    {
                        Balance = Converters.ConvertBalance(new Balance
                        {
                            Currency = "XBT", Value = (balance * 0.00000001).ToString(CultureInfo.InvariantCulture)
                        }),
                        Sessionid = meta.GetValue("sessionid"),
                        Time = new Timestamp { Seconds = DateTimeOffset.Now.ToUnixTimeSeconds() }
                    }
                }, meta);
            }

            await ConnectionTester(PublishBalanceUpdateFunc);
            return response;
        }

        /// <summary>
        /// Обновляет инофрмацию об ордере в сервис истории. Это может быть информация об инициализации, добавлении, обновлении, исполнении как своего ордера, так и контр-ордера.
        /// </summary>
        internal async Task<PublishEventResponse> WriteOrder(Order order, ChangesType changesType, Metadata meta,
            string message)
        {
            PublishEventResponse response = null;

            async Task PublishOrderUpdateFunc()
            {
                response = await _client.PublishEventAsync(new PublishEventRequest
                {
                    Order = new PublishOrderEvent
                    {
                        ChangesType = (TradeBot.Common.v1.ChangesType)changesType,
                        Order = Converters.ConvertOrder(order), 
                        Sessionid = meta.GetValue("sessionid"),
                        Time = new Timestamp { Seconds = DateTimeOffset.Now.ToUnixTimeSeconds() },
                        Message = message, SlotName = meta.GetValue("slot")
                    }
                },meta);
            }

            await ConnectionTester(PublishOrderUpdateFunc);
            return response;
        }

    }
}
