using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.Model;

namespace TradeMarket.DataTransfering
{
    public class FakeSlotSubscriber : ISubscriber<Slot> , IFakeDataSubscriber
    {
        private static FakeSlotSubscriber _fakeSlotSubscriber = null;

        public static FakeSlotSubscriber GetInstance()
        {
            if(_fakeSlotSubscriber == null)
            {
                _fakeSlotSubscriber = new FakeSlotSubscriber();
            }
            return _fakeSlotSubscriber;
        }

        private List<Slot> slots = new List<Slot>
        {
            new Slot
            {
                 Currency1 = new ("USD1"),
                 Currency2 = new ("XBT1"),
                 Name = "XBTUSD1"
            },
            new Slot
            {
                 Currency1 = new ("USD2"),
                 Currency2 = new ("XBT2"),
                 Name = "XBTUSD2"
            },
            new Slot
            {
                 Currency1 = new ("USD3"),
                 Currency2 = new ("XBT3"),
                 Name = "XBTUSD3"
            },
            new Slot
            {
                 Currency1 = new ("USD4"),
                 Currency2 = new ("XBT4"),
                 Name = "XBTUSD4"
            },
            new Slot
            {
                 Currency1 = new ("USD5"),
                 Currency2 = new ("XBT5"),
                 Name = "XBTUSD5"
            },
            new Slot
            {
                 Currency1 = new ("USD6"),
                 Currency2 = new ("XBT6"),
                 Name = "XBTUSD6"
            },
            new Slot
            {
                 Currency1 = new ("USD7"),
                 Currency2 = new ("XBT7"),
                 Name = "XBTUSD7"
            },
        };

        public event ISubscriber<Slot>.ChangedEventHandler Changed;

        public async Task Simulate()
        {
            foreach(var slot in slots)
            {
                await Task.Delay(new Random().Next(2000));
                Changed?.Invoke(this, new (slot));
            }
        }
    }
}
