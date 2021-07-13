using System.Threading.Tasks;

namespace TradeMarket.DataTransfering
{
    internal interface FakeDataSubscriber
    {
        public Task Simulate();
    }
}