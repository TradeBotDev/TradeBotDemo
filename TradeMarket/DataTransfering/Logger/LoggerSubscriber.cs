using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering
{
    //TODO Денис пока без логов.
    public class LoggerSubscriber<T> : IPublisher<ILogger<T>>
    {
        public event EventHandler<IPublisher<ILogger<T>>.ChangedEventArgs> Changed;
    }
}
