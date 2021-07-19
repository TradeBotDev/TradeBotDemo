using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.Model;

namespace TradeMarket.DataTransfering
{
    public class FakeBalanceSubscriber : ISubscriber<Balance>, IFakeDataSubscriber
    {
        private static FakeBalanceSubscriber _fakeBalanceSubscriber = null;
        private FakeBalanceSubscriber()
        {

        }

        public static FakeBalanceSubscriber GetInstance()
        {
            if(_fakeBalanceSubscriber == null)
            {
                _fakeBalanceSubscriber = new FakeBalanceSubscriber();
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

        public event ISubscriber<Balance>.ChangedEventHandler Changed;

        public async Task Simulate()
        {
            var random = new Random();
            foreach (var order in _balanceChangingInTime)
            {
                await Task.Delay(random.Next(0, 2000));
                Changed?.Invoke(this, new(order));
            }
        }

    }

    
}
