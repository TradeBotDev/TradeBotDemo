using Grpc.Core;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Former
{
    public class UserContext
    {
        //Класс контекста пользователя
        public string SessionId => Meta.GetValue("sessionid");
        public string TradeMarket => Meta.GetValue("trademarket");
        public string Slot => Meta.GetValue("slot");
        public Logger Logger { get; }
        private Config _configuration;
        private readonly Storage _storage;
        private readonly TradeMarketClient _tradeMarketClient;
        private readonly Former _former;
        private readonly UpdateHandlers _updateHandlers;

        public Metadata Meta { get; }

        internal UserContext(string sessionId, string tradeMarket, string slot, Config configuration)
        {
            _configuration = configuration;
            Meta = new Metadata
            {
                { "sessionid", sessionId },
                { "trademarket", tradeMarket },
                { "slot", slot }
            };
            TradeMarketClient.Configure("https://localhost:5005", 10000);

            Logger = new Logger();
            _tradeMarketClient = new TradeMarketClient();
            _storage = new Storage(Logger);
            _former = new Former(_storage, _configuration, _tradeMarketClient, Meta, Logger);
            _updateHandlers = new UpdateHandlers(_storage, _configuration, _tradeMarketClient, Meta, Logger);

            _tradeMarketClient.UpdateMarketPrices += _storage.UpdateMarketPrices;
            _tradeMarketClient.UpdateBalance += _storage.UpdateBalance;
            _tradeMarketClient.UpdateMyOrders += _storage.UpdateMyOrderList;
            _tradeMarketClient.UpdatePosition += _storage.UpdatePosition;

            _tradeMarketClient.Start(Meta);
        }

        public async Task FormOrder(int decision)
        {
            await _former.FormOrder(decision);
        }
    }
}
