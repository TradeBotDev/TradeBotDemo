using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Facade
{
    interface IConnectionInfo<T> where T: class
    {
        GrpcChannel Channel { get; }
        T Client { get; }
        void RemoveConnection();
        void AddConnection();
    }
    
}
