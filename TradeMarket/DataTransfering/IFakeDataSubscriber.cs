using System.Threading.Tasks;

namespace TradeMarket.DataTransfering
{
    internal interface IFakeDataSubscriber
    {
        public Task Simulate();
    }
}