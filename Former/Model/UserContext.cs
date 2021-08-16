using System;
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
        private readonly Storage _storage;
        private readonly TradeMarketClient _tradeMarketClient;
        private readonly Former _former;
        private readonly UpdateHandlers _updateHandlers;
        private readonly HistoryClient _historyClient;
        private bool _subscribesAttached;
        

        public Metadata Meta { get; }

        internal UserContext(string sessionId, string tradeMarket, string slot)
        {
            Meta = new Metadata
            {
                { "sessionid", sessionId },
                { "trademarket", tradeMarket },
                { "slot", slot }
            };
            HistoryClient.Configure(Environment.GetEnvironmentVariable("HISTORY_CONNECTION_STRING"), int.TryParse(Environment.GetEnvironmentVariable("RETRY_DELAY"), out var retryDelay) ? retryDelay : retryDelay = 10000);
            _historyClient = new HistoryClient();

            TradeMarketClient.Configure(Environment.GetEnvironmentVariable("TRADEMARKET_CONNECTION_STRING"), retryDelay);
            _tradeMarketClient = new TradeMarketClient();

            _storage = new Storage(_historyClient, Meta);
            _former = new Former(_storage, null, _tradeMarketClient, Meta, _historyClient);
            _updateHandlers = new UpdateHandlers(_storage, null, _tradeMarketClient, Meta, _historyClient);

            SubscribeStorageToMarket();
        }

        public void SubscribeStorageToMarket()
        {
            if (_subscribesAttached) return;
            _tradeMarketClient.UpdateMarketPrices += _storage.UpdateMarketPrices;
            _tradeMarketClient.UpdateBalance += _storage.UpdateBalance;
            _tradeMarketClient.UpdateMyOrders += _storage.UpdateMyOrderList;
            _tradeMarketClient.UpdatePosition += _storage.UpdatePosition;
            _tradeMarketClient.StartObserving(Meta);
            _subscribesAttached = true;
            Log.Information("{@Where}: Former has been started!", "Former");
        }

        public void UnsubscribeStorage()
        {
            if (!_subscribesAttached) return;
            _tradeMarketClient.StopObserving();
            _tradeMarketClient.UpdateMarketPrices -= _storage.UpdateMarketPrices;
            _tradeMarketClient.UpdateBalance -= _storage.UpdateBalance;
            _tradeMarketClient.UpdateMyOrders -= _storage.UpdateMyOrderList;
            _tradeMarketClient.UpdatePosition -= _storage.UpdatePosition;
            ClearStorage();
            _subscribesAttached = false;
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
        }

        public async Task FormOrder(int decision)
        {
            await _former.FormOrder(decision);
        }

        public async Task RemoveAllMyOrders()
        {
            await _former.RemoveAllMyOrders();
        }
        
        public void SetConfiguration(Config configuration)
        {
            _former.SetConfiguration(configuration);
            _updateHandlers.SetConfiguration(configuration);
        }
    }
}
