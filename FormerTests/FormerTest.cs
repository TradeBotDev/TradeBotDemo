using Former;
using TradeBot.Common.v1;
using Xunit;
using BindingFlags = System.Reflection.BindingFlags;
using Former = Former.Former;

namespace FormerTests
{
    public class FormerTest
    {
        [Fact]
        public void FormOrderTest()
        {
            var type = typeof(UserContext);
            var ctor = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var result = ctor[0].Invoke(new object[] { "sessionId", "bitmex", "XBTUSD" });

            var storage = type.GetField("_storage", BindingFlags.NonPublic | BindingFlags.Instance);
            var configuration = type.GetField("Configuration");

            var testStorage = new Storage
            {
                AvailableBalance = 1220000,
                TotalBalance = 1220000,
                BuyMarketPrice = 38000,
                SellMarketPrice = 38000.5,
            };
            var testConfig = new Config
            {
                AvaibleBalance = 1.0,
                RequiredProfit = 0.01,
                ContractValue = 100,
                OrderUpdatePriceRange = 1
            };
            storage?.SetValue(result, testStorage);
            configuration?.SetValue(result, testConfig);
            

        }

    }
}
