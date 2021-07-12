using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Former
{
    public class AlgorithmAnswerService : Algorithm.AlgorithmAnswerService.AlgorithmAnswerServiceClient
    {
        private readonly ILogger<AlgorithmAnswerService> _logger;
        public AlgorithmAnswerService(ILogger<AlgorithmAnswerService> logger)
        {
            _logger = logger;
        }
    }
}
