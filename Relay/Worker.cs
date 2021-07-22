using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Relay.Clients;

namespace Relay
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly AlgorithmClientService _algorithm;
        private readonly TradeMarketClientService _tradeMarket;

        public Worker(ILogger<Worker> logger,AlgorithmClientService algorithm,TradeMarketClientService tradeMarket)
        {
            _logger = logger;
            this._algorithm = algorithm;
            this._tradeMarket = tradeMarket;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(3000);
            _tradeMarket.OrderRecievedEvent += async (sender, args) =>
            {
                _logger.LogInformation($"Sending order {args}");
                await _algorithm.WriteOrder(args);
            };
            while (!stoppingToken.IsCancellationRequested)
            {

                await Task.Delay(1000, stoppingToken);

               
            }
        }
    }
}
