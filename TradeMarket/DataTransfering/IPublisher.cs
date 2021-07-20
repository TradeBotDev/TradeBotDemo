using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering
{
    /// <summary>
    /// Отвечает за оповещение об изменениях в выбранных типах
    /// </summary>
    /// <typeparam name="T">класс за изменением которого идет отслеживание</typeparam>
    public interface IPublisher<T>
    {
        public enum Action
        {
            Partial,
            Insert,
            Update,
            Delete
        }
        public class ChangedEventArgs : EventArgs
        {
            
            public Action Action;
            public T Changed { get; internal set; }

            public ChangedEventArgs(T changed,Action action)
            {
                this.Action = action;
                this.Changed = changed;
                //this.Changed = Convert(changed);
            }
        }

        //TODO я все еще не уверен чья обязанность конвернировать внутренние типы в те которые будут по сети кидаться
        //TODO сюда бы добавить Converter<LocalClass,Reply> как-нибудь
        /*public static Reply Convert(LocalClass localReply)
        {
            throw new NotImplementedException();
        }*/
        public event EventHandler<ChangedEventArgs> Changed;
    }
}
