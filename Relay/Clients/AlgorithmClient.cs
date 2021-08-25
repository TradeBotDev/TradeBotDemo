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
using Serilog;

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
        public IClientStreamWriter<AddOrderRequest> ReConncet(Metadata meta)
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
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task UpdateConfig(TradeBot.Common.v1.UpdateServerConfigRequest update ,Metadata meta)
        {
            while (true)
            {
                try
                {
                    await _client.UpdateServerConfigAsync(new UpdateServerConfigRequest()
                    {
                        Request = new TradeBot.Common.v1.UpdateServerConfigRequest()
                        {
                            Config = update.Config,
                            Switch = update.Switch
                        }
                    }, meta);
                    break;

                }
                catch (Exception e)
                {
                    Log.ForContext("sessionId", meta.GetValue("sessionid")).ForContext("slot", meta.GetValue("slot")).Error("{@Where}: Exception {@Exception}","Relay", e.Message);
                    await Task.Delay(5000);
                }
            }
        }
        
    }
}
