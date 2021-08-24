using Bitmex.Client.Websocket.Responses;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Model.Publishers
{
    public interface IPublisher<T>
    {

        public bool IsWorking { get; }
        public List<T> Cache { get; set; }

        public class ChangedEventArgs : EventArgs
        {
            
            public BitmexAction Action { get; }
            public T Changed { get; }

            public ChangedEventArgs(T changed,BitmexAction action)
            {
                this.Action = action;
                this.Changed = changed;
            }
        }
      
        public event EventHandler<ChangedEventArgs> Changed;

        public int SubscribersCount { get; }

        public Task Start(ILogger logger);

        public Task Stop(ILogger logger);
    }
}
