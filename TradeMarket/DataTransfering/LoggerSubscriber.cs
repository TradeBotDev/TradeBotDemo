using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering
{
    //TODO Денис пока без логов.
    public class LoggerSubscriber<T> : Subscriber<ILogger<T>>
    {
        public event Subscriber<ILogger<T>>.ChangedEventHandler Changed;
    }
}
