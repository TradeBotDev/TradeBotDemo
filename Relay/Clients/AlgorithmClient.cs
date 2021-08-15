using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using TradeBot.Algorithm.AlgorithmService.v1;
using TradeBot.Common.v1;
using UpdateServerConfigRequest = TradeBot.Algorithm.AlgorithmService.v1.UpdateServerConfigRequest;
using Grpc.Net.ClientFactory;
namespace Relay.Clients
{
    public class AlgorithmClient
    {
        private readonly AlgorithmService.AlgorithmServiceClient _client;
        
        public AlgorithmClient(string uri)
        {
           _client = new AlgorithmService.AlgorithmServiceClient(GrpcChannel.ForAddress(uri));
        }

        public IClientStreamWriter<AddOrderRequest> OpenStream(Metadata meta)
        {
            return _client.AddOrder(meta).RequestStream;
        }

        public async Task WriteOrder(IClientStreamWriter<AddOrderRequest> stream,Order order)
        {
            try
            {
               await stream.WriteAsync(new AddOrderRequest()
               {
                   Order = order
               });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task UpdateConfig(TradeBot.Common.v1.UpdateServerConfigRequest update ,Metadata meta)
        {
            await _client.UpdateServerConfigAsync(new UpdateServerConfigRequest()
            {
                Request = new TradeBot.Common.v1.UpdateServerConfigRequest()
                {
                    Config = update.Config,
                    Switch =update.Switch
                }
            },meta);
        }
        
    }
}
