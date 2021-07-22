using System.Threading.Tasks;

namespace TradeMarket.DataTransfering
{
    internal interface IFakeDataPublisher
    {
        public Task Simulate();
    }
}