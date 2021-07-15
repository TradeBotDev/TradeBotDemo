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
        //private static TradeBot.Common.v1.Config config;
        public Service(ILogger<Service> logger)
        {
            _logger = logger;
        }
        public override Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request, ServerCallContext context)
        {
            Former.config = request.Request;
            return Task.FromResult(new UpdateServerConfigResponse());
        }
    }
}
