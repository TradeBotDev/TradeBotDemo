using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.Former.FormerService.v1;

namespace Relay.Clients
{
    public class FormerClient
    {
        private readonly FormerService.FormerServiceClient _client;

        public FormerClient(Uri uri)
        {
            _client = new FormerService.FormerServiceClient(GrpcChannel.ForAddress(uri));
        }

        public async Task UpdateConfig(Config config,Metadata meta)
        {
             await _client.UpdateServerConfigAsync(new() {Request = config }, meta);
        }

        
    }
}

