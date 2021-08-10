using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
using TradeBot.Common.v1;
using TradeBot.History.HistoryService.v1;

namespace Former.Clients
{
    public class HistoryClient
    {
        private static int _retryDelay;
        private static string _connectionString;
        private CancellationTokenSource _token;

        private readonly HistoryService.HistoryServiceClient _client;

        public static void Configure(string connectionString, int retryDelay)
        {
            _connectionString = connectionString;
            _retryDelay = retryDelay;
        }

        public HistoryClient()
        {
            _client = new HistoryService.HistoryServiceClient(GrpcChannel.ForAddress(_connectionString));
        }

        /// <summary>
        /// Проверяет соединение с биржей, на вход принимает функцию, осуществляющую общение с биржей
        /// </summary>
        private async Task ConnectionTester(Func<Task> func)
        {
            while (true)
            {
                try
                {
                    await func.Invoke();
                    break;
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Error {@ExceptionMessage}. Retrying...\r\n{@ExceptionStackTrace}", "Former", e.Message, e.StackTrace);
                    Thread.Sleep(_retryDelay);
                }
            }
        }

        internal async Task<PublishBalanceUpdateResponse> WriteBalance(double balance, Metadata meta)
        {
            PublishBalanceUpdateResponse response = null;
            async Task PublishBalanceUpdateFunc()
            {
                response = await _client.PublishBalanceUpdateAsync(new PublishBalanceUpdateRequest
                {
                    Balance = new Balance { Currency = "XBT", Value = balance.ToString(CultureInfo.InvariantCulture) },
                    Sessionid = meta.GetValue("sessionid"), Time = new Timestamp { Seconds = DateTimeOffset.Now.ToUnixTimeSeconds() }
                });
            }

            await ConnectionTester(PublishBalanceUpdateFunc);
            return response;
        }

        internal async Task<PublishOrderUpdateResponse> WriteOrder(Order order, ChangesType changesType, Metadata meta, string message)
        {
            PublishOrderUpdateResponse response = null;

            async Task PublishBalanceUpdateFunc()
            {
                response = await _client.PublishOrderUpdateAsync(new PublishOrderUpdateRequest()
                {
                    ChangesType = changesType, Order = order, Sessionid = meta.GetValue("sessionid"),
                    Time = new Timestamp { Seconds = DateTimeOffset.Now.ToUnixTimeSeconds() },
                    Message = message
                });
            }

            await ConnectionTester(PublishBalanceUpdateFunc);
            return response;

        }

    }
}
