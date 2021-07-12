using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.Model;

/// <summary>
///Этот класс должен отслеживать изменения данных на бирже и отсылать их другим сервисам
/// </summary>
namespace TradeMarket.DataTransfering
{
    public delegate void OrdersAddedEventHandler(object sender, OrdersAddedEventArgs args);

    public class OrdersAddedEventArgs : EventArgs
    {
        public IEnumerable<FullOrder> addedOrders { get; internal set; }

        public OrdersAddedEventArgs(IEnumerable<FullOrder> addedOrders)
        {
            this.addedOrders = addedOrders;
        }
    }

    public class Transferrer
    {
        private static Transferrer _transferrer = null;
        public static Transferrer GetInstance()
        {
            if(_transferrer == null)
            {
                _transferrer = new Transferrer();
            }
            return _transferrer;
        }
        public event OrdersAddedEventHandler ordersChanged;


        private List<FullOrder> sampleOrders = new List<FullOrder>
        {
            //TODO написать набор оредров которые будут выкидываться подписчикам
            new FullOrder
            {
                Id = "1",
                Quantity = 2,
                Price = 16.2D,
                RemovePrice = 15D,
                LastUpdateDate = DateTime.Now.AddDays(3),
                RemoveDate = DateTime.Now.AddDays(4),
                Signature = new TradeBot.Common.OrderSignature
                {
                    Status = TradeBot.Common.OrderStatus.Open,
                    Type = TradeBot.Common.OrderType.Buy
                }
            }
        };

        public async Task FillOrdersAsync()
        {
            var random = new Random();
            foreach(var order in sampleOrders)
            {
                await Task.Delay(random.Next(0,2000));
                ordersChanged(this, new OrdersAddedEventArgs(new List<FullOrder> { order }));
            }
        }
        
    }
}
