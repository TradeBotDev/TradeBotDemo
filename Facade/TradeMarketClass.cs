using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RequestAndResponse = TradeBot.TradeMarket.TradeMarketService.v1;
using TMClient = TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient;

namespace Facade
{
    public class TradeMarketClass : IConnectionInfo<TMClient>
    {
        private GrpcChannel _channel => GrpcChannel.ForAddress("https://localhost:5005");
        public GrpcChannel Channel { get => _channel; }

        private TMClient _client=>new TMClient(Channel);
        public TMClient Client
        {
            get => _client;
        }

        public void RemoveConnection()
        {
            
        }

        public void AddConnection()
        {
            throw new NotImplementedException();
        }
    }
    public static class MyExtension
    {
        public static AsyncServerStreamingCall<RequestAndResponse.SubscribeBalanceResponse> SubscribeBalance(this TMClient cl, RequestAndResponse.SubscribeBalanceRequest request)
        {
            var response = cl.SubscribeBalance(new RequestAndResponse.SubscribeBalanceRequest { Request = request.Request, SlotName = request.SlotName });
            //if (response =)
            return response;
        }
    }
}
