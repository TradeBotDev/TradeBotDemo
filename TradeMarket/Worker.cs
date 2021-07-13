using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.DataTransfering;

namespace TradeMarket
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);

                //запуск подписок
                Task[] tasks =
                {
                    FakeOrderSubscriber.GetInstance().Simulate(),
                    FakeBalanceSubscriber.GetInstance().Simulate()
                };
                await Task.WhenAll(tasks);
            }
        }
    }
}