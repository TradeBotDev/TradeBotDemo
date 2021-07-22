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
    public class AlgorithmClientService
    {
        private bool _isOn = false;

        public bool IsOn
        {
            get
            {
                return _isOn;
            }
            set
            {
                if (value)
                {
                    //TradeMarketClientService.OrderRecievedEvent += TradeMarketClientService_OrderRecievedEvent;
                }
                else
                {
                    //TradeMarketClientService.OrderRecievedEvent -= TradeMarketClientService_OrderRecievedEvent;

                }
                _isOn = value;
            }
        }

        private readonly IClientStreamWriter<AddOrderRequest> _stream;
        private readonly AlgorithmService.AlgorithmServiceClient _client;
        
        public AlgorithmClientService(Uri uri)
        {
           
            _client = new AlgorithmService.AlgorithmServiceClient(GrpcChannel.ForAddress(uri));
            _stream = _client.AddOrder().RequestStream;

        }

        public async Task WriteOrder(Order order)
        {
            try
            {
               await _stream.WriteAsync(new AddOrderRequest()
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

        public async Task UpdateConfig(Config config)
        {
            await _client.UpdateServerConfigAsync(new UpdateServerConfigRequest()
            {
                Request = new TradeBot.Common.v1.UpdateServerConfigRequest()
                {
                    Config = config
                }
            });
        }
        
    }
}
