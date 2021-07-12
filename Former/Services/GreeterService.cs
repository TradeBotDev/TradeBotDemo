using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Former
{
    public class Former : Former
    {
        private readonly ILogger<Former> _logger;
        public Former(ILogger<Former> logger)
        {
            _logger = logger;
        }
    }
}
