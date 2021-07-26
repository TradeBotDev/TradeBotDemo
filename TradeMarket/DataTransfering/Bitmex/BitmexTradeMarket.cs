using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Communicator;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeMarket.DataTransfering.Bitmex.Publishers;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests;
using TradeMarket.Model;
using Bitmex.Client.Websocket.Responses.Orders;
using Utf8Json;
using OrderStatus = TradeBot.Common.v1.OrderStatus;
using TradeMarket.DataTransfering.Bitmex.Rest.Responses;

namespace TradeMarket.DataTransfering.Bitmex
{
    public class BitmexTradeMarket : Model.TradeMarket
    {
        private BookPublisher Book25Publisher;
        private BookPublisher BookPublisher;
        private UserOrderPublisher UserOrdersPublisher;
        private UserWalletPublisher UserWalletPublisher;
        private AuthenticationPublisher UserAuthenticationPublisher;

        public override event EventHandler<FullOrder> Book25Update;
        public override event EventHandler<FullOrder> BookUpdate;
        public override event EventHandler<FullOrder> UserOrdersUpdate;
        public override event EventHandler<Model.Balance> BalanceUpdate;

        public BitmexTradeMarket(string name)
        {
            Name = name;
        }

        private void _userWalletPublisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Wallets.Wallet>.ChangedEventArgs e)
        {

            BalanceUpdate?.Invoke(sender, new Model.Balance(e.Changed.Currency, e.Changed.BalanceBtc));
        }

        private void _userOrdersPublisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Orders.Order>.ChangedEventArgs e)
        {
            UserOrdersUpdate?.Invoke(sender, ConverFromOrder(e));
        }

        private void _bookPublisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Books.BookLevel>.ChangedEventArgs e)
        {
            BookUpdate?.Invoke(sender, ConvertFromBookLevel(e));
        }

        private void _book25Publisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Books.BookLevel>.ChangedEventArgs e)
        {
            Book25Update?.Invoke(sender, ConvertFromBookLevel(e));
        }

        private FullOrder ConverFromOrder(IPublisher<global::Bitmex.Client.Websocket.Responses.Orders.Order>.ChangedEventArgs e)
        {
            FullOrder order = new FullOrder
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
            FullOrder order = new FullOrder();
            OrderSignature signature = new OrderSignature()
            {
                Status = GetSignatureStatusFromAction(e.Action),
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

        public async override Task<DefaultResponse> AutheticateUser(string api, string secret, UserContext context)
        {
            if(UserAuthenticationPublisher is null){
                UserAuthenticationPublisher = new AuthenticationPublisher(context.WSClient, context.WSClient.Streams.AuthenticationStream);
            }
            bool answer = false;
            UserAuthenticationPublisher.Changed += (sender, args) => { answer = args.Changed; };

            await UserAuthenticationPublisher.SubcribeAsync(context.Key, context.Secret, new System.Threading.CancellationToken());
            return new DefaultResponse
            { 
                Code = answer ? ReplyCode.Succeed : ReplyCode.Failure,
                Message = ""
            };
        }

        //TODO дописать 
        public override Task<DefaultResponse> CloseOrder(string id,UserContext context)
        {
            throw new NotImplementedException();
        }

        public async override Task<DefaultResponse> PlaceOrder(double quontity, double price,UserContext context)
        {
            var response = await context.RestClient.SendAsync(new PlaceOrderRequest(context.Key, context.Secret, new global::Bitmex.Client.Websocket.Responses.Orders.Order
            {
                //TODO тут переделать под лимит ордер
                OrdType = "Limit",
                Price = price,
                OrderQty = (long?)quontity,
                Symbol = context.SlotName
            }),new System.Threading.CancellationToken());
            string message = "";
            string responsejson = await response.Content.ReadAsStringAsync();
            ReplyCode code = ReplyCode.Succeed;
            if (!response.IsSuccessStatusCode)
            {

                responsejson = responsejson.Replace("{\"error\":", "");
                responsejson = responsejson.Remove(responsejson.LastIndexOf("}") - 1, 1);
                message = JsonSerializer.Deserialize<error>(responsejson).message;
                code = ReplyCode.Failure;
            }
            else
            {
                message = responsejson;
            }
            return new DefaultResponse
            {
                Message = message,
                Code = code
            };
        }

        public async override void SubscribeToBook25(EventHandler<FullOrder> handler, UserContext context)
        {
            if(Book25Publisher is null)
            {
                Book25Publisher = new BookPublisher(context.WSClient, context.WSClient.Streams.Book25Stream);
                Book25Publisher.Changed += _book25Publisher_Changed;
            }
            await Book25Publisher.SubscribeAsync(new Book25SubscribeRequest(context.SlotName), new System.Threading.CancellationToken());
            Book25Update += handler;
        }

        public async override void SubscribeToBook(EventHandler<FullOrder> handler, UserContext context)
        {
            if (BookPublisher is null)
            {
                BookPublisher = new BookPublisher(context.WSClient, context.WSClient.Streams.BookStream);
                BookPublisher.Changed += _bookPublisher_Changed;
            }
            await BookPublisher.SubscribeAsync(new BookSubscribeRequest(context.SlotName), new System.Threading.CancellationToken());
            BookUpdate += handler;
        }

        public async override void SubscribeToUserOrders(EventHandler<FullOrder> handler, UserContext context)
        {
            if (UserOrdersPublisher is null)
            {
                UserOrdersPublisher = new UserOrderPublisher(context.WSClient, context.WSClient.Streams.OrderStream);
                UserOrdersPublisher.Changed += _userOrdersPublisher_Changed;
            }
            await UserOrdersPublisher.SubcribeAsync(new System.Threading.CancellationToken());
            UserOrdersUpdate += handler;
        }

        public async override void SubscribeToBalance(EventHandler<Model.Balance> handler, UserContext context)
        {
            if (UserWalletPublisher is null)
            {
                UserWalletPublisher = new UserWalletPublisher(context.WSClient, context.WSClient.Streams.WalletStream);
                UserWalletPublisher.Changed += _userWalletPublisher_Changed;
            }
            await UserWalletPublisher.SubcribeAsync(new System.Threading.CancellationToken());
            BalanceUpdate += handler;
        }
    }
}
