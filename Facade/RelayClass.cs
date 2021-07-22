using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Relay.RelayService.v1;

namespace Facade
{
    public class RelayClass : IConnectionInfo<RelayService.RelayServiceClient>
    {
        public GrpcChannel Channel => throw new NotImplementedException();

        public RelayService.RelayServiceClient Client { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void AddConnection()
        {
            throw new NotImplementedException();
        }

        public void RemoveConnection()
        {
            throw new NotImplementedException();
        }
    }
}
