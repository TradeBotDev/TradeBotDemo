using System.Threading;
using System.Threading.Tasks;
using Former.Clients;
using Grpc.Core;
using TradeBot.Common.v1;

namespace Former.Model
{
    public class UserContext
    {
        //Класс контекста пользователя
        public string SessionId => Meta.GetValue("sessionid");
        public string TradeMarket => Meta.GetValue("trademarket");
        public string Slot => Meta.GetValue("slot");
        private Config _configuration;
        private readonly Storage _storage;
        private readonly TradeMarketClient _tradeMarketClient;
        private readonly Former _former;
        private readonly UpdateHandlers _updateHandlers;
        private readonly HistoryClient _historyClient;
        

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

            _historyClient = new HistoryClient();
            _tradeMarketClient = new TradeMarketClient();
            _storage = new Storage(_historyClient);
            _former = new Former(_storage, _configuration, _tradeMarketClient, Meta, _historyClient);
            _updateHandlers = new UpdateHandlers(_storage, _configuration, _tradeMarketClient, Meta, _historyClient);

            SubscribeStorageToMarket();
        }

        public void SubscribeStorageToMarket()
        {
            _tradeMarketClient.UpdateMarketPrices += _storage.UpdateMarketPrices;
            _tradeMarketClient.UpdateBalance += _storage.UpdateBalance;
            _tradeMarketClient.UpdateMyOrders += _storage.UpdateMyOrderList;
            _tradeMarketClient.UpdatePosition += _storage.UpdatePosition;
            _tradeMarketClient.StartObserving(Meta);
        }

        public void UnsubscribeStorage()
        {
            _tradeMarketClient.StopObserving();
            _tradeMarketClient.UpdateMarketPrices -= _storage.UpdateMarketPrices;
            _tradeMarketClient.UpdateBalance -= _storage.UpdateBalance;
            _tradeMarketClient.UpdateMyOrders -= _storage.UpdateMyOrderList;
            _tradeMarketClient.UpdatePosition -= _storage.UpdatePosition;
        }

        public async Task FormOrder(int decision)
        {
            await _former.FormOrder(decision);
        }

        public async Task RemoveAllMyOrders()
        {
            await _former.RemoveAllMyOrders();
        }
    }
}
