using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeBot.Common;
using TradeBot.Former.FormerService.v1;

namespace Former
{
    public class Service : FormerService.FormerServiceBase
    {
        private readonly ILogger<Service> _logger;
        public Service(ILogger<Service> logger)
        {
            _logger = logger;
        }
        //private override 
    }
}
