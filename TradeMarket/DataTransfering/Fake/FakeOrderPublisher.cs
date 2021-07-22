using Bitmex.Client.Websocket.Responses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeMarket.Model;

namespace TradeMarket.DataTransfering
{
  
    public class FakeOrderPublisher : IPublisher<FullOrder>,IFakeDataPublisher
    {
        private static FakeOrderPublisher _fakeOrderSubscriber = null;

        private FakeOrderPublisher() { }

        public static FakeOrderPublisher GetInstance()
        {
            if(_fakeOrderSubscriber == null)
            {
                _fakeOrderSubscriber = new FakeOrderPublisher();
            }
            return _fakeOrderSubscriber;
        }

        private List<FullOrder> sampleOrders = new List<FullOrder>
        {
            //TODO написать набор оредров которые будут выкидываться подписчикам
            new FullOrder
            {
                Id = "1",
                Quantity = 2,
                Price = 4.2D,
                RemovePrice = 15D,
                LastUpdateDate = DateTime.Now.AddDays(3),
                RemoveDate = DateTime.Now.AddDays(4),
                Signature = new OrderSignature
                {
                    Status = OrderStatus.Open,
                    Type = OrderType.Buy
                }
            },
            new FullOrder
            {
                Id = "2",
                Quantity = 2,
                Price = 9.2D,
                RemovePrice = 15D,
                LastUpdateDate = DateTime.Now.AddDays(3),
                RemoveDate = DateTime.Now.AddDays(4),
                Signature = new OrderSignature
                {
                    Status = OrderStatus.Open,
                    Type = OrderType.Buy
                }
            },
            new FullOrder
            {
                Id = "3",
                Quantity = 2,
                Price = 16.2D,
                RemovePrice = 15D,
                LastUpdateDate = DateTime.Now.AddDays(3),
                RemoveDate = DateTime.Now.AddDays(4),
                Signature = new OrderSignature
                {
                    Status = OrderStatus.Open,
                    Type = OrderType.Buy
                }
            },
            new FullOrder
            {
                Id = "4",
                Quantity = 2,
                Price = 20.2D,
                RemovePrice = 15D,
                LastUpdateDate = DateTime.Now.AddDays(3),
                RemoveDate = DateTime.Now.AddDays(4),
                Signature = new OrderSignature
                {
                    Status = OrderStatus.Open,
                    Type = OrderType.Buy
                }
            },
            new FullOrder
            {
                Id = "5",
                Quantity = 2,
                Price = 8.2D,
                RemovePrice = 15D,
                LastUpdateDate = DateTime.Now.AddDays(3),
                RemoveDate = DateTime.Now.AddDays(4),
                Signature = new OrderSignature
                {
                    Status = OrderStatus.Open,
                    Type = OrderType.Buy
                }
            },
            new FullOrder
            {
                Id = "6",
                Quantity = 2,
                Price = 18.2D,
                RemovePrice = 15D,
                LastUpdateDate = DateTime.Now.AddDays(3),
                RemoveDate = DateTime.Now.AddDays(4),
                Signature = new OrderSignature
                {
                    Status = OrderStatus.Open,
                    Type = OrderType.Buy
                }
            },
            new FullOrder
            {
                Id = "7",
                Quantity = 2,
                Price = 40.2D,
                RemovePrice = 15D,
                LastUpdateDate = DateTime.Now.AddDays(3),
                RemoveDate = DateTime.Now.AddDays(4),
                Signature = new OrderSignature
                {
                    Status = OrderStatus.Open,
                    Type = OrderType.Buy
                }
            }
        };

        public event EventHandler<IPublisher<FullOrder>.ChangedEventArgs> Changed;

        public async Task Simulate()
        {
            var random = new Random();
            foreach(var order in sampleOrders)
            {
                await Task.Delay(random.Next(0,2000));
                Changed?.Invoke(this, new (order, BitmexAction.Undefined));
            }
        }
        
    }
}
