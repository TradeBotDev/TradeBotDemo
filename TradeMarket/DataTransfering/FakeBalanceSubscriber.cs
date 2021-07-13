using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.Model;

namespace TradeMarket.DataTransfering
{
    public class FakeBalanceSubscriber : Subscriber<Balance>
    {
        private List<Balance> balanceChangingInTime = new List<Balance>
        {
            //у меня тут один биток равен одному доллару
            new Balance("USD",3D),
            new Balance("XBT",2D),
            new Balance("USD",4D),
            new Balance("XBT",1D)
        };

        public event Subscriber<Balance>.ChangedEventHandler Changed;

        public async Task SimulateOrders()
        {
            var random = new Random();
            foreach (var order in balanceChangingInTime)
            {
                await Task.Delay(random.Next(0, 2000));
                Changed(this, new(order));
            }
        }

    }

    
}
