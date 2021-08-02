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
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Wallets;
using StackExchange.Redis;

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
        private UserPositionPublisher _userPositionPublisher;


        private IConnectionMultiplexer _multiplexer;

        public override event EventHandler<IPublisher<BookLevel>.ChangedEventArgs> Book25Update;
        public override event EventHandler<IPublisher<BookLevel>.ChangedEventArgs> BookUpdate;
        public override event EventHandler<IPublisher<Order>.ChangedEventArgs> UserOrdersUpdate;
        public override event EventHandler<Wallet> BalanceUpdate;
        public override event EventHandler<IPublisher<Margin>.ChangedEventArgs> MarginUpdate;
        public override event EventHandler<IPublisher<Position>.ChangedEventArgs> PositionUpdate;


        public BitmexTradeMarket(string name,IConnectionMultiplexer multiplexer)
        {
            _multiplexer = multiplexer;
            Name = name;
        }

        #region EventsHandlers
        private void _userPositionPublisher_Changed(object sender, IPublisher<Position>.ChangedEventArgs e)
        {
            //Log.Information("Recieved Position {@Position}", e.Changed);
            PositionUpdate?.Invoke(this, e);
        }


        private void _userMarginPublisher_Changed(object sender, IPublisher<Margin>.ChangedEventArgs e)
        {
            //Log.Information("Recieved Margin {@Margin}", e);
            MarginUpdate?.Invoke(sender, e);
        }


        private void _userWalletPublisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Wallets.Wallet>.ChangedEventArgs e)
        {
            //Log.Information("Recieved Balance {@Balance}", e);
            BalanceUpdate?.Invoke(sender, e.Changed);
        }

        private void _userOrdersPublisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Orders.Order>.ChangedEventArgs e)
        {
            UserOrdersUpdate?.Invoke(sender, e );
        }

        private void _bookPublisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Books.BookLevel>.ChangedEventArgs e)
        {
            BookUpdate?.Invoke(sender, e);
        }

        private void _book25Publisher_Changed(object sender, IPublisher<global::Bitmex.Client.Websocket.Responses.Books.BookLevel>.ChangedEventArgs e)
        {
            Book25Update?.Invoke(sender, e);
        }
        #endregion

        #region SubscribeRequests
        public async override void SubscribeToUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler, UserContext context)
        {
            if (_userPositionPublisher is null)
            {
                _userPositionPublisher = new UserPositionPublisher(context.WSClient, context.WSClient.Streams.PositionStream);
                _userPositionPublisher.Changed += _userPositionPublisher_Changed;
            }
            await _userPositionPublisher.SubcribeAsync(new System.Threading.CancellationToken());
            PositionUpdate += handler;
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

        public async override void SubscribeToBook25(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler, UserContext context)
        {
            if(_book25Publisher is null)
            {
                _book25Publisher = new BookPublisher(context.WSClient, context.WSClient.Streams.Book25Stream,_multiplexer);
                _book25Publisher.Changed += _book25Publisher_Changed;
            }
            await _book25Publisher.SubscribeAsync(new Book25SubscribeRequest(context.SlotName), new System.Threading.CancellationToken());
            Book25Update += handler;
        }

        public async override void SubscribeToBook(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler, UserContext context)
        {
            if (_bookPublisher is null)
            {
                _bookPublisher = new BookPublisher(context.WSClient, context.WSClient.Streams.BookStream,_multiplexer);
                _bookPublisher.Changed += _bookPublisher_Changed;
            }
            await _bookPublisher.SubscribeAsync(new BookSubscribeRequest(context.SlotName), new System.Threading.CancellationToken());
            BookUpdate += handler;
        }

        public async override void SubscribeToUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler, UserContext context)
        {
            if (_userOrdersPublisher is null)
            {
                _userOrdersPublisher = new UserOrderPublisher(context.WSClient, context.WSClient.Streams.OrderStream);
                _userOrdersPublisher.Changed += _userOrdersPublisher_Changed;
            }
            await _userOrdersPublisher.SubcribeAsync(new System.Threading.CancellationToken());
            UserOrdersUpdate += handler;
        }

        public async override void SubscribeToBalance(EventHandler<Wallet> handler, UserContext context)
        {
            if (_userWalletPublisher is null)
            {
                _userWalletPublisher = new UserWalletPublisher(context.WSClient, context.WSClient.Streams.WalletStream);
                _userWalletPublisher.Changed += _userWalletPublisher_Changed;
            }
            await _userWalletPublisher.SubcribeAsync(new System.Threading.CancellationToken());
            BalanceUpdate += handler;
        }
        #endregion

        #region RestREquests
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
            if (!string.IsNullOrEmpty(orderMessage.OrdRejReason))
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
        public async override Task<DefaultResponse> DeleteOrder(string id, UserContext context)
        {
            var response = await context.RestClient.SendAsync(new DeleteOrderRequest(context.Key, context.Secret, id), new System.Threading.CancellationToken());
            if (response.Error is not null)
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

        //TODO перенести в конвертер
        public static double RoundToHalfOrZero(double value)
        {
            double truncatedDifference = value - Math.Truncate(value);
            //если что-то случилось и пришло не ровно 0.5
            //разница с 0.5 больше чем 0.01 считается нарошной 
            if (Math.Abs(truncatedDifference - 0.5) < 0.01)
            {
                return Math.Truncate(value) + 0.5;
            }
            else
            {
                return Math.Truncate(value);
            }
        }

        public async override Task<TradeBot.TradeMarket.TradeMarketService.v1.PlaceOrderResponse> PlaceOrder(double quontity, double price, UserContext context)
        {
            var response = await context.RestClient.SendAsync(new PlaceOrderRequest(context.Key, context.Secret, new Order
            {
                Symbol = context.SlotName,
                OrdType = "Limit",
                Price = RoundToHalfOrZero(price),
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
            if (!string.IsNullOrEmpty(orderMessage.OrdRejReason))
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
        #endregion
    }
}
