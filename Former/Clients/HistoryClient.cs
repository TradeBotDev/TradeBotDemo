using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
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
                    if (e.StatusCode == StatusCode.Cancelled) break;
                    Log.Error("{@Where}: Error {@ExceptionMessage}. Retrying...\r\n{@ExceptionStackTrace}", "Former", e.Message, e.StackTrace);
                    Thread.Sleep(_retryDelay);
                }
            }
        }



    }
}
