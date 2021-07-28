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
using Serilog;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests.Place;
using Newtonsoft.Json;
using Order = Bitmex.Client.Websocket.Responses.Orders.Order;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests.Ammend;
using Bitmex.Client.Websocket.Responses.Margins;
using TradeBot.TradeMarket.TradeMarketService.v1;
using Margin = Bitmex.Client.Websocket.Responses.Margins.Margin;
using DeleteOrderRequest = TradeMarket.DataTransfering.Bitmex.Rest.Requests.DeleteOrderRequest;
using PlaceOrderRequest = TradeMarket.DataTransfering.Bitmex.Rest.Requests.Place.PlaceOrderRequest;
using AmmendOrderRequest = TradeMarket.DataTransfering.Bitmex.Rest.Requests.Ammend.AmmendOrderRequest;

namespace TradeMarket.DataTransfering.Bitmex
{
    public class BitmexTradeMarket : Model.TradeMarket
    {
        private BookPublisher _book25Publisher;
        private BookPublisher _bookPublisher;
        private UserOrderPublisher _userOrdersPublisher;
        private UserWalletPublisher _userWalletPublisher;
        private AuthenticationPublisher _userAuthenticationPublisher;
        private UserMarginPublisher _userMarginPublisher;

        public override event EventHandler<FullOrder> Book25Update;
        public override event EventHandler<FullOrder> BookUpdate;
        public override event EventHandler<Order> UserOrdersUpdate;
        public override event EventHandler<Model.Balance> BalanceUpdate;
        public override event EventHandler<IPublisher<Margin>.ChangedEventArgs> MarginUpdate;

        public BitmexTradeMarket(string name)
        {
            Name = name;
        }

        public async override void SubscribeToUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler, UserContext context)
        {
            if (_userMarginPublisher is null)
            {
                _userMarginPublisher = new UserMarginPublisher(context.WSClient, context.WSClient.Streams.MarginStream);
                _userMarginPublisher.Changed += _userMarginPublisher_Changed;
            }
            await _userMarginPublisher.SubcribeAsync(new System.Threading.CancellationToken());
            MarginUpdate += handler;
        }

        private void _userMarginPublisher_Changed(object sender, IPublisher<Margin>.ChangedEventArgs e)
        {
            Log.Information("Recieved Margin {@Margin}", e);
            MarginUpdate?.Invoke(sender, e);
        }


        private void _userWalletPublisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Wallets.Wallet>.ChangedEventArgs e)
        {
            Log.Information("Recieved Balance {@Balance}", e);
            BalanceUpdate?.Invoke(sender, new Model.Balance(e.Changed.Currency, e.Changed.BalanceBtc));
        }

        private void _userOrdersPublisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Orders.Order>.ChangedEventArgs e)
        {
            UserOrdersUpdate?.Invoke(sender, e.Changed );
        }

        private void _bookPublisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Books.BookLevel>.ChangedEventArgs e)
        {
            BookUpdate?.Invoke(sender, ConvertFromBookLevel(e));
        }

        private void _book25Publisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Books.BookLevel>.ChangedEventArgs e)
        {
            Book25Update?.Invoke(sender, ConvertFromBookLevel(e));
        }

        public FullOrder ConverFromOrder(IPublisher<global::Bitmex.Client.Websocket.Responses.Orders.Order>.ChangedEventArgs e)
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
            if(_userAuthenticationPublisher is null){
                _userAuthenticationPublisher = new AuthenticationPublisher(context.WSClient, context.WSClient.Streams.AuthenticationStream);
            }
            bool answer = false;
            _userAuthenticationPublisher.Changed += (sender, args) => { answer = args.Changed; };

            await _userAuthenticationPublisher.SubcribeAsync(context.Key, context.Secret, new System.Threading.CancellationToken());
            return new DefaultResponse
            { 
                Code = answer ? ReplyCode.Succeed : ReplyCode.Failure,
                Message = ""
            };
        }

        public async override Task<DefaultResponse> DeleteOrder(string id, UserContext context)
        {
            var response = await context.RestClient.SendAsync(new DeleteOrderRequest(context.Key, context.Secret, id), new System.Threading.CancellationToken());
            if(response.Error is not null)
            {
                return new()
                {
                    Code = ReplyCode.Failure,
                    Message = response.Error.Message
                };
            }
            var orderMessage = response.Message;
            return new()
            {
                Code = ReplyCode.Succeed,
                Message = $"Order {orderMessage.OrderId} was deleted"
            };
        }

        public async override Task<TradeBot.TradeMarket.TradeMarketService.v1.PlaceOrderResponse> PlaceOrder(double quontity, double price,UserContext context)
        {
            var response = await context.RestClient.SendAsync(new PlaceOrderRequest(context.Key, context.Secret, new Order
            {
                Symbol = context.SlotName,
                OrdType = "Limit",
                Price = (int)price,
                OrderQty = (long?)quontity
            }), new System.Threading.CancellationToken());
            if (response.Error is not null)
            {
                return new()
                {
                    OrderId = "empty",
                    Response = new()
                    {
                        Code = ReplyCode.Failure,
                        Message = response.Error.Message
                    }
                };
            }
            var orderMessage = response.Message;
            if(!string.IsNullOrEmpty(orderMessage.OrdRejReason))
            {
                return new()
                {
                    OrderId = orderMessage.OrderId,
                    Response = new()
                    {
                        Code = ReplyCode.Failure,
                        Message = orderMessage.OrdRejReason
                    }
                };
            }
            return new()
            {
                OrderId = orderMessage.OrderId,
                Response = new()
                {
                    Code = ReplyCode.Succeed,
                    Message = $"Order Placed"
                }
            };
        }

        public async override void SubscribeToBook25(EventHandler<FullOrder> handler, UserContext context)
        {
            if(_book25Publisher is null)
            {
                _book25Publisher = new BookPublisher(context.WSClient, context.WSClient.Streams.Book25Stream);
                _book25Publisher.Changed += _book25Publisher_Changed;
            }
            await _book25Publisher.SubscribeAsync(new Book25SubscribeRequest(context.SlotName), new System.Threading.CancellationToken());
            Book25Update += handler;
        }

        public async override void SubscribeToBook(EventHandler<FullOrder> handler, UserContext context)
        {
            if (_bookPublisher is null)
            {
                _bookPublisher = new BookPublisher(context.WSClient, context.WSClient.Streams.BookStream);
                _bookPublisher.Changed += _bookPublisher_Changed;
            }
            await _bookPublisher.SubscribeAsync(new BookSubscribeRequest(context.SlotName), new System.Threading.CancellationToken());
            BookUpdate += handler;
        }

        public async override void SubscribeToUserOrders(EventHandler<Order> handler, UserContext context)
        {
            if (_userOrdersPublisher is null)
            {
                _userOrdersPublisher = new UserOrderPublisher(context.WSClient, context.WSClient.Streams.OrderStream);
                _userOrdersPublisher.Changed += _userOrdersPublisher_Changed;
            }
            await _userOrdersPublisher.SubcribeAsync(new System.Threading.CancellationToken());
            UserOrdersUpdate += handler;
        }

        public async override void SubscribeToBalance(EventHandler<Model.Balance> handler, UserContext context)
        {
            if (_userWalletPublisher is null)
            {
                _userWalletPublisher = new UserWalletPublisher(context.WSClient, context.WSClient.Streams.WalletStream);
                _userWalletPublisher.Changed += _userWalletPublisher_Changed;
            }
            await _userWalletPublisher.SubcribeAsync(new System.Threading.CancellationToken());
            BalanceUpdate += handler;
        }

        public async override Task<DefaultResponse> AmmendOrder(string id, double? price, long? Quantity, long? LeavesQuantity, UserContext context)
        {
            var response = await context.RestClient.SendAsync(new AmmendOrderRequest(context.Key, context.Secret, new() {Id = id, LeavesQuantity = LeavesQuantity, Price = price, Quantity = Quantity }), new System.Threading.CancellationToken());
            if (response.Error is not null)
            {
                return new()
                {
                    Code = ReplyCode.Failure,
                    Message = response.Error.Message
                };
            }
            var orderMessage = response.Message;
            if (orderMessage.OrdRejReason is not null)
            {
                return new()
                {
                    Code = ReplyCode.Failure,
                    Message = orderMessage.OrdRejReason
                };
            }
            return new()
            {
                Code = ReplyCode.Succeed,
                Message = $"Order {orderMessage.OrderId} Ammended"
            };
        }
    }
}
