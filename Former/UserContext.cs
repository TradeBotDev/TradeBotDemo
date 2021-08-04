using Grpc.Core;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;

namespace Former
{
    public class UserContext
    {
        //Класс контекста пользователя
        public readonly string SessionId;
        public readonly string TradeMarket;
        public readonly string Slot;
        public Config Configuration;
        private readonly TradeMarketClient _tradeMarketClient;
        private readonly Former _former;
        private readonly Storage _storage;
        private readonly UpdateHandlers _updateHandlers;

        public Metadata Meta { get; }

        internal UserContext(string sessionId, string tradeMarket, string slot)
        {
            SessionId = sessionId;
            TradeMarket = tradeMarket;
            Slot = slot;

            Meta = new Metadata
            {
                { "sessionid", sessionId },
                { "trademarket", tradeMarket },
                { "slot", slot }
            };

            TradeMarketClient.Configure("https://localhost:5005", 10000);

            _tradeMarketClient = new TradeMarketClient();
            _storage = new Storage();
            _former = new Former(_storage);
            _updateHandlers = new UpdateHandlers(_storage);

            _tradeMarketClient.UpdateMarketPrices += UpdateMarketPrices;
            _tradeMarketClient.UpdateBalance += UpdateBalance;
            _tradeMarketClient.UpdateMyOrders += UpdateMyOrderList;
            _tradeMarketClient.UpdatePosition += UpdatePosition;


            _ = ObserveMarketPrices();
            _ = ObserveBalance();
            _ = ObserveMyOrders();
            _ = ObservePositions();
        }

        private async Task UpdateMarketPrices(double bid, double ask)
        {
            await _storage.UpdateMarketPrices(bid, ask, this);
        }
        private async Task UpdateMyOrderList(Order orderNeededUpdate, ChangesType changesType)
        {
            await _storage.UpdateMyOrderList(orderNeededUpdate, changesType, this);
        }
        private async Task UpdateBalance(int balanceToBuy, int balanceToSell)
        {
            await _storage.UpdateBalance(balanceToBuy, balanceToSell);
        }
        private async Task UpdatePosition(double currentQuantity)
        {
            await _storage.UpdatePosition(currentQuantity);
        }
        public async Task FormOrder(int decision)
        {
            await _former.FormOrder(decision, this);
        }
        public async Task<PlaceOrderResponse> PlaceOrder(double sellPrice, double contractValue)
        {
            return await _tradeMarketClient.PlaceOrder(sellPrice, contractValue, this);
        }
        public async Task<AmmendOrderResponse> AmendOrder(string id, double newPrice)
        {
            return await _tradeMarketClient.AmendOrder(id, newPrice, this);
        }
        private async Task ObserveBalance()
        {
            await _tradeMarketClient.ObserveBalance(this);
        }
        private async Task ObserveMyOrders()
        {
            await _tradeMarketClient.ObserveMyOrders(this);
        }
        private async Task ObservePositions()
        {
            await _tradeMarketClient.ObservePositions(this);
        }
        private async Task ObserveMarketPrices()
        {
            await _tradeMarketClient.ObserveMarketPrices(this);
        }
    }
}
