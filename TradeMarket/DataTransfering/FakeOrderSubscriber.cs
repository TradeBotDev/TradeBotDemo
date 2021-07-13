﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.Model;

namespace TradeMarket.DataTransfering
{
  
    public class FakeOrderSubscriber : Subscriber<FullOrder>,FakeDataSubscriber
    {
        private static FakeOrderSubscriber _fakeOrderSubscriber = null;

        private FakeOrderSubscriber() { }

        public static FakeOrderSubscriber GetInstance()
        {
            if(_fakeOrderSubscriber == null)
            {
                _fakeOrderSubscriber = new FakeOrderSubscriber();
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
                Price = 16.2D,
                RemovePrice = 15D,
                LastUpdateDate = DateTime.Now.AddDays(3),
                RemoveDate = DateTime.Now.AddDays(4),
                Signature = new TradeBot.Common.OrderSignature
                {
                    Status = TradeBot.Common.OrderStatus.Open,
                    Type = TradeBot.Common.OrderType.Buy
                }
            },
            new FullOrder
            {
                Id = "2",
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
            },
            new FullOrder
            {
                Id = "3",
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
            },
            new FullOrder
            {
                Id = "4",
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
            },
            new FullOrder
            {
                Id = "5",
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
            },
            new FullOrder
            {
                Id = "6",
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
            },
            new FullOrder
            {
                Id = "7",
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

        public event Subscriber<FullOrder>.ChangedEventHandler Changed;

        public async Task Simulate()
        {
            var random = new Random();
            foreach(var order in sampleOrders)
            {
                await Task.Delay(random.Next(0,2000));
                Changed(this, new (order));
            }
        }
        
    }
}