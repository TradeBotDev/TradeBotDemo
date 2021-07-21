using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Responses.Orders;

using System;
using System.Threading.Tasks;

using TradeBot.Common.v1;

using TradeMarket.DataTransfering.Bitmex.Publishers;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests;
using TradeMarket.Model;

using Utf8Json;

namespace TradeMarket.DataTransfering.Bitmex
{
    public class BitmexTradeMarket : TradeMarket
    {
        private BookPublisher _book25Publisher;
        private BookPublisher _bookPublisher;
        private UserOrderPublisher _userOrdersPublisher;
        private UserWalletPublisher _userWalletPublisher;
        private AuthenticationPublisher _userAuthenticationPublisher;

        public BitmexTradeMarket()
        {
        }

        private void _userWalletPublisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Wallets.Wallet>.ChangedEventArgs e)
        {

            BalanceUpdated?.Invoke(sender, new Model.Balance(e.Changed.Currency, e.Changed.BalanceBtc));
        }

        private void _userOrdersPublisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Orders.Order>.ChangedEventArgs e)
        {
            UserOrdersUpdated?.Invoke(sender, ConverFromOrder(e));
        }

        private void _bookPublisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Books.BookLevel>.ChangedEventArgs e)
        {
            BookUpdated?.Invoke(sender, ConvertFromBookLevel(e));
        }

        private void _book25Publisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Books.BookLevel>.ChangedEventArgs e)
        {
            Book25Updated?.Invoke(sender, ConvertFromBookLevel(e));
        }

        private FullOrder ConverFromOrder(IPublisher<global::Bitmex.Client.Websocket.Responses.Orders.Order>.ChangedEventArgs e)
        {
            var order = new FullOrder
            {
                Signature = new OrderSignature
                {
                    Status = GetSignatureStatusFromAction(e.Action),
                    Type = e.Changed.Side == BitmexSide.Buy ? OrderType.Buy : OrderType.Sell
                },
                Id = e.Changed.OrderId,
                LastUpdateDate = DateTime.Now,
                Price = e.Changed.Price.HasValue ? e.Changed.Price.Value : default(double),
                Quantity = e.Changed.OrderQty.HasValue ? (int)e.Changed.OrderQty.Value : default(int)
            };
            return order;

        }

        private TradeBot.Common.v1.OrderStatus GetSignatureStatusFromAction(BitmexAction action)
        {
            switch (action)
            {
                case BitmexAction.Partial: return TradeBot.Common.v1.OrderStatus.Open;
                case BitmexAction.Insert: return TradeBot.Common.v1.OrderStatus.Open;
                case BitmexAction.Delete: return TradeBot.Common.v1.OrderStatus.Closed;
                case BitmexAction.Update: return TradeBot.Common.v1.OrderStatus.Open;
            }
            return TradeBot.Common.v1.OrderStatus.Unspecified;
        }

        private FullOrder ConvertFromBookLevel(IPublisher<global::Bitmex.Client.Websocket.Responses.Books.BookLevel>.ChangedEventArgs e)
        {
            var order = new FullOrder();
            var signature = new OrderSignature()
            {
                Status = OrderStatus.Unspecified,
                Type = e.Changed.Side == BitmexSide.Buy ? OrderType.Buy : OrderType.Sell
            };
            order.Id = e.Changed.Id.ToString();
            //это время последнего обновления. оно пока что берется с текущей даты
            order.LastUpdateDate = DateTime.Now;
            order.Price = e.Changed.Price.HasValue ? (double)e.Changed.Price : default(double);
            order.Quantity = e.Changed.Size.HasValue ? (int)e.Changed.Size : default(int);

            order.Signature = signature;
            return order;
        }

        public override async Task<DefaultResponse> AutheticateUser(string api, string secret, BitmexUserContext context)
        {
            if (_userAuthenticationPublisher is null)
            {
                _userAuthenticationPublisher = new AuthenticationPublisher(context.WSClient, context.WSClient.Streams.AuthenticationStream);
            }
            var answer = false;
            _userAuthenticationPublisher.Changed += (sender, args) => { answer = args.Changed; };

            await _userAuthenticationPublisher.SubcribeAsync(context.Key, context.Secret, new System.Threading.CancellationToken());
            return new DefaultResponse
            {
                Code = answer ? ReplyCode.Succeed : ReplyCode.Failure,
                Message = ""
            };
        }

        //TODO дописать 
        public override Task<DefaultResponse> CloseOrder(string id, BitmexUserContext context)
        {
            throw new NotImplementedException();
        }

        public override async Task<DefaultResponse> PlaceOrder(double quontity, double price, BitmexUserContext context)
        {
            var response = await context.RestClient.SendAsync(new PlaceOrderRequest(context.Key, context.Secret, new global::Bitmex.Client.Websocket.Responses.Orders.Order
            {
                OrdType = "Sell",
                Price = price,
                OrderQty = (long?)quontity
            }), new System.Threading.CancellationToken());
            var message = "";
            var code = ReplyCode.Succeed;
            if (!response.IsSuccessStatusCode)
            {
                message = JsonSerializer.Deserialize<ErrorResponse>(await response.Content.ReadAsStringAsync()).Error;
                code = ReplyCode.Failure;
            }
            return new DefaultResponse
            {
                Message = message,
                Code = code
            };
        }

        public override async void SubscribeToBook25(EventHandler<FullOrder> handler, BitmexUserContext context)
        {
            if (_book25Publisher is null)
            {
                _book25Publisher = new BookPublisher(context.WSClient, context.WSClient.Streams.Book25Stream);
                _book25Publisher.Changed += _book25Publisher_Changed;
            }
            await _book25Publisher.SubscribeAsync(new BookSubscribeRequest("XBTUSD"), new System.Threading.CancellationToken());
            //Book25Updated += handler;
        }

        public override async void SubscribeToBook(EventHandler<FullOrder> handler, BitmexUserContext context)
        {
            if (_bookPublisher is null)
            {
                _bookPublisher = new BookPublisher(context.WSClient, context.WSClient.Streams.BookStream);
                _bookPublisher.Changed += _bookPublisher_Changed;
            }
            await _bookPublisher.SubscribeAsync(new BookSubscribeRequest("XBTUSD"), new System.Threading.CancellationToken());
            //BookUpdated += handler;
        }

        public override async void SubscribeToUserOrders(EventHandler<FullOrder> handler, BitmexUserContext context)
        {
            if (_userOrdersPublisher is null)
            {
                _userOrdersPublisher = new UserOrderPublisher(context.WSClient, context.WSClient.Streams.OrderStream);
                _userOrdersPublisher.Changed += _userOrdersPublisher_Changed;
            }
            await _userOrdersPublisher.SubcribeAsync(new System.Threading.CancellationToken());
            //UserOrdersUpdated += handler;
        }

        public override async void SubscribeToBalance(EventHandler<Model.Balance> handler, BitmexUserContext context)
        {
            if (_userWalletPublisher is null)
            {
                _userWalletPublisher = new UserWalletPublisher(context.WSClient, context.WSClient.Streams.WalletStream);
                _userWalletPublisher.Changed += _userWalletPublisher_Changed;
            }
            await _userWalletPublisher.SubcribeAsync(new System.Threading.CancellationToken());
            //BalanceUpdated += handler;
        }
    }
}
