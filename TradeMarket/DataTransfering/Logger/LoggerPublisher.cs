using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.Model.Publishers;

namespace TradeMarket.DataTransfering
{
    //TODO Денис пока без логов.
    public class LoggerPublisher<T> : IPublisher<ILogger<T>>
    {
        public event EventHandler<IPublisher<ILogger<T>>.ChangedEventArgs> Changed;

        public void Start()
        {
            throw new NotImplementedException();
        }

        Task IPublisher<ILogger<T>>.Start()
        {
            throw new NotImplementedException();
        }
    }
}
