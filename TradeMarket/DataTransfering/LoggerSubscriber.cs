using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering
{
    //TODO Денис пока без логов.
    public class LoggerSubscriber<T> : ISubscriber<ILogger<T>>
    {
        public event ISubscriber<ILogger<T>>.ChangedEventHandler Changed;
    }
}
