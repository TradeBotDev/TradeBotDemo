using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace History.Cache
{
    public class RedisReader
    {
        private readonly IDatabase _db;
        public RedisReader(IConnectionMultiplexer multiplexer)
        {
            _db = multiplexer.GetDatabase();
        }


    }
}
