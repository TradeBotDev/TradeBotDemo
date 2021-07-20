using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Communicator;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeMarket.DataTransfering.Bitmex.Publishers;
using TradeMarket.Model;

namespace TradeMarket.DataTransfering.Bitmex
{
    public class BitmexTradeMarket : Model.TradeMarketBase
    {
        private BookPublisher _book25Publisher;
        private BookPublisher _bookPublisher;
        private UserOrderPublisher _userOrdersPublisher;
        private UserWalletPublisher _userWalletPublisher;
        private AuthenticationPublisher _userAuthenticationPublisher;

        private BitmexWebsocketClient _wsClient;
        //TODO тут можно сделать переключение между тестнетом и обычным типом для тестирования алгоритма
        private IBitmexCommunicator _wsCommunicator;


        public BitmexTradeMarket()
        {
            _wsCommunicator = new BitmexWebsocketCommunicator(BitmexValues.ApiWebsocketUrl);
            _wsClient = new BitmexWebsocketClient(_wsCommunicator);

            //хардок на наш тестовый акк
            string key = "lVjs4QoJIe9OqNUnDoVKl2jS";
            string secret = "MAX6lma-Y93bfT3w-g5GtAgvsFNhDLrYlyyqkciDUwRTy64s";

            _userAuthenticationPublisher = new AuthenticationPublisher(_wsClient, _wsClient.Streams.AuthenticationStream);
            //пока не подписываемся на аутентификацию пользователя
            _userAuthenticationPublisher.Changed += (sender, args) => { };
            _userAuthenticationPublisher.SubcribeAsync(key,secret,new System.Threading.CancellationToken());

            //ХАРДКООООООООООООООООООООООООООООООООООООД
            _book25Publisher = new BookPublisher(_wsClient, _wsClient.Streams.Book25Stream);
            _book25Publisher.Changed += _book25Publisher_Changed;
            _bookPublisher = new BookPublisher(_wsClient, _wsClient.Streams.BookStream);
            _bookPublisher.Changed += _bookPublisher_Changed;
            _userOrdersPublisher = new UserOrderPublisher(_wsClient, _wsClient.Streams.OrderStream);
            _userOrdersPublisher.Changed += _userOrdersPublisher_Changed;
            _userWalletPublisher = new UserWalletPublisher(_wsClient, _wsClient.Streams.WalletStream);
            _userWalletPublisher.Changed += _userWalletPublisher_Changed;
        }

        private void _userWalletPublisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Wallets.Wallet>.ChangedEventArgs e)
        {
            
            throw new NotImplementedException();
        }

        private void _userOrdersPublisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Orders.Order>.ChangedEventArgs e)
        {
            
            throw new NotImplementedException();
        }

        private void _bookPublisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Books.BookLevel>.ChangedEventArgs e)
        {
            BookUpdated?.Invoke(sender, Convert(e));
        }

        private void _book25Publisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Books.BookLevel>.ChangedEventArgs e)
        {
            Book25Updated?.Invoke(sender, Convert(e));
        }

        private FullOrder Convert(IPublisher<global::Bitmex.Client.Websocket.Responses.Books.BookLevel>.ChangedEventArgs e)
        { 
            FullOrder order = new FullOrder();
            OrderSignature signature = new OrderSignature()
            {
                Status = OrderStatus.Unspecified,
                Type = e.Changed.Side == BitmexSide.Buy ? OrderType.Buy : OrderType.Sell
            };
            order.Id = e.Changed.Id.ToString();
            //это время последнего обновления. оно пока что берется с текущей даты
            order.LastUpdateDate = DateTime.Now;
            order.Price = e.Changed.Price.HasValue ? (double)e.Changed.Price : default(double);
            order.Quantity = e.Changed.Size.HasValue ? (int)e.Changed.Size : default(int);
            switch (e.Action)
            {
                case BitmexAction.Partial: signature.Status = OrderStatus.Open; break;
                case BitmexAction.Insert: signature.Status = OrderStatus.Open; break;
                case BitmexAction.Delete: signature.Status = OrderStatus.Closed; break;
                case BitmexAction.Update: signature.Status = OrderStatus.Open; break;
            }
            order.Signature = signature;
            return order;
        }

       

        public override event EventHandler<FullOrder> Book25Updated;
        public override event EventHandler<FullOrder> BookUpdated;
        public override event EventHandler<FullOrder> UserOrdersUpdated;
        public override event EventHandler<Model.Balance> BalanceUpdated;

        public override Task<bool> AutheticateUser(string api, string secret)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> CloseOrder(string id)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> PlaceOrder(double quontity, double price)
        {
            throw new NotImplementedException();
        }
    }
}
