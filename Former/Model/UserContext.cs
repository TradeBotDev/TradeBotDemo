using System.Threading;
using System.Threading.Tasks;
using Former.Clients;
using Grpc.Core;
using Serilog;
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
            HistoryClient.Configure("http://localhost:5007", 10000);
            _historyClient = new HistoryClient();

            TradeMarketClient.Configure("https://localhost:5005", 10000);
            _tradeMarketClient = new TradeMarketClient();
            _storage = new Storage(_historyClient);
            _former = new Former(_storage, _configuration, _tradeMarketClient, Meta, _historyClient);
            _updateHandlers = new UpdateHandlers(_storage, _configuration, _tradeMarketClient, Meta, _historyClient);
        }

        public void SubscribeStorageToMarket()
        {
            _tradeMarketClient.UpdateMarketPrices += _storage.UpdateMarketPrices;
            _tradeMarketClient.UpdateBalance += _storage.UpdateBalance;
            _tradeMarketClient.UpdateMyOrders += _storage.UpdateMyOrderList;
            _tradeMarketClient.UpdatePosition += _storage.UpdatePosition;
            _tradeMarketClient.StartObserving(Meta);
            Log.Information("{@Where}: Former has been started!", "Former");
        }

        public void UnsubscribeStorage()
        {
            _tradeMarketClient.StopObserving();
            _tradeMarketClient.UpdateMarketPrices -= _storage.UpdateMarketPrices;
            _tradeMarketClient.UpdateBalance -= _storage.UpdateBalance;
            _tradeMarketClient.UpdateMyOrders -= _storage.UpdateMyOrderList;
            _tradeMarketClient.UpdatePosition -= _storage.UpdatePosition;
            ClearStorage();
            Log.Information("{@Where}: Former has been stopped!", "Former");
        }

        private void ClearStorage()
        {
            _storage.MyOrders.Clear();
            _storage.CounterOrders.Clear();
            _storage.AvailableBalance = 0;
            _storage.SellMarketPrice = 0;
            _storage.TotalBalance = 0;
            _storage.PositionSize = 0;
            _storage.BuyMarketPrice = 0;
            _storage.FitPricesLocker = false;
            _storage.PlaceLocker = false;
            _storage.SavedMarketBuyPrice = 0;
            _storage.SavedMarketSellPrice = 0;
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
