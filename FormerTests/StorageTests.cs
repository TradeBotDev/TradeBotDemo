using System.Threading.Tasks;
using Former;
using Xunit;


namespace FormerTests
{
    public class StorageTests
    {
        [Fact]
        public async Task UpdateMarketPrices_Bid10AndAskNegative1_Bid10AndAsk0returned()
        {
            //Arrange
            var logger = new Logger();
            var storage = new Storage(logger)
            {
                BuyMarketPrice = 0,
                SellMarketPrice = 0
            };

            //Act
            await storage.UpdateMarketPrices(10, -1);
            
            //Assert
            Assert.Equal(10,storage.BuyMarketPrice);
            Assert.Equal(0,storage.SellMarketPrice);
        }
        [Fact]
        public async Task UpdateMarketPrices_BidNegative1AndAsk0_Bid0andAsk10returned()
        {
            //Arrange
            var logger = new Logger();
            var storage = new Storage(logger)
            {
                BuyMarketPrice = 0,
                SellMarketPrice = 0
            };

            //Act
            await storage.UpdateMarketPrices(-1, 10);
            
            //Assert
            Assert.Equal(0,storage.BuyMarketPrice);
            Assert.Equal(10,storage.SellMarketPrice);
        }
        [Fact]
        public async Task UpdateMarketPrices_Bid10AndAsk0_Bid10andAsk10returned()
        {
            //Arrange
            var logger = new Logger();
            var storage = new Storage(logger)
            {
                BuyMarketPrice = 0,
                SellMarketPrice = 0
            };

            //Act
            await storage.UpdateMarketPrices(10, 10);
            
            //Assert
            Assert.Equal(10,storage.BuyMarketPrice);
            Assert.Equal(10,storage.SellMarketPrice);
        }


    }
}
