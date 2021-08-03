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

        public Metadata Meta { get; private set; }

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
            _former = new Former();

            _tradeMarketClient.UpdateBalance += UpdateBalance;
            _tradeMarketClient.UpdateMyOrders += UpdateMyOrderList;
            _tradeMarketClient.UpdatePosition += UpdatePosition;
            _tradeMarketClient.UpdateMarketPrices += UpdateMarketPrices;

            ObserveMarketPrices();
            ObserveBalance();
            ObserveMyOrders();
            ObservePositions();
        }

        private async void UpdateMarketPrices(double bid, double ask)
        {
            await _former.UpdateMarketPrices(bid, ask, this);
            
        }
        private async void ObservePositions()
        {
            await _tradeMarketClient.ObservePositions(this);
        }
        public async void ObserveMarketPrices()
        {
            await _tradeMarketClient.ObserveMarketPrices(this);
        }
        public async void FormBuyOrder()
        {
            await _former.FormBuyOrder(this);
        }
        public async void FormSellOrder()
        {
            await _former.FormSellOrder(this);
        }
        private async void UpdateMyOrderList(Order orderNeededUpdate, ChangesType changesType)
        {
            await _former.UpdateMyOrderList(orderNeededUpdate, changesType, this);
        }
        private async void UpdateBalance(int balanceToBuy, int balanceToSell)
        {
            await _former.UpdateBalance(balanceToBuy, balanceToSell);
        }
        private async void UpdatePosition(double currentQuantity)
        {
            await _former.UpdatePosition(currentQuantity);
        }
        public async Task<PlaceOrderResponse> PlaceOrder(double sellPrice, double contractValue)
        {
            return await _tradeMarketClient.PlaceOrder(sellPrice, contractValue, this);
        }
        public async Task<AmmendOrderResponse> AmendOrder(string id, double newPrice)
        {
            return await _tradeMarketClient.AmendOrder(id, newPrice, this);
        }
        private async void ObserveBalance()
        {
            await _tradeMarketClient.ObserveBalance(this);
        }
        private async void ObserveMyOrders()
        {
            await _tradeMarketClient.ObserveMyOrders(this);
        }
    }
}
