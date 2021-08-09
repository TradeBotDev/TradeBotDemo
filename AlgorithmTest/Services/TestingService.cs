using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using Grpc.Core;
using TradeBot.Algorithm.AlgorithmTestingService.v1;

namespace AlgorithmTest
{
    public class AAA : TestingService.TestingServiceBase
    {
        Order order = new Order();
    }
}
