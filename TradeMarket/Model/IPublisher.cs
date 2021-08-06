using Bitmex.Client.Websocket.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering
{
    public interface IPublisher<T>
    {
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

        public void Start();
    }
}
