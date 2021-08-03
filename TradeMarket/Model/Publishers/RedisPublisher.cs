using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Model.Publishers
{
    public class RedisPublisher
    {
        public String IdPrefix { get; }
        public String TopicPrefix { get; }
    }

    public enum IdPostfixRule {
        Date,subjectId, AccountId
    }

    public enum TopicPostfixRule {
        SessionId
    }


}
