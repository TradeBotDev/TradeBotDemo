using Bitmex.Client.Websocket.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.Model;

namespace TradeMarket.DataTransfering
{
    public class FakeBalancePublisher : IPublisher<Balance>, IFakeDataPublisher
    {
        private static FakeBalancePublisher _fakeBalanceSubscriber = null;
        private FakeBalancePublisher()
        {

        }

        public static FakeBalancePublisher GetInstance()
        {
            if(_fakeBalanceSubscriber == null)
            {
                _fakeBalanceSubscriber = new FakeBalancePublisher();
            }
            return _fakeBalanceSubscriber;
        }

        private List<Balance> _balanceChangingInTime = new List<Balance>
        {
            //у меня тут один биток равен одному доллару
            new Balance("USD",3D),
            new Balance("XBT",2D),
            new Balance("USD",4D),
            new Balance("XBT",1D)
        };

        public event EventHandler<IPublisher<Balance>.ChangedEventArgs> Changed;

        public async Task Simulate()
        {
            var random = new Random();
            foreach (var order in _balanceChangingInTime)
            {
                await Task.Delay(random.Next(0, 2000));
                Changed?.Invoke(this, new(order,BitmexAction.Undefined));
            }
        }

    }

    
}
