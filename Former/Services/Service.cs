using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeBot.Common;
using TradeMarket.Former;

namespace Former
{
    public class Service : Algorithm.Former.AlgorithmObserverService.AlgorithmObserverServiceClient
    {
        private readonly ILogger<Service> _logger;
        public Service(ILogger<Service> logger)
        {
            _logger = logger;
        }
    }
}
