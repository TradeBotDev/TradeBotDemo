﻿using System;
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
        private readonly AlgorithmClient _algorithm;
        private readonly TradeMarketClient _tradeMarket;

        public Worker(ILogger<Worker> logger,AlgorithmClient algorithm,TradeMarketClient tradeMarket)
        {
            _logger = logger;
            this._algorithm = algorithm;
            this._tradeMarket = tradeMarket;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var raiseService = new RaiseService();
            return base.StartAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                await Task.Delay(1000, stoppingToken);

               
            }
        }
    }
}
