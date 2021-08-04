using System;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using Serilog;
using Order = Bitmex.Client.Websocket.Responses.Orders.Order;
using Margin = Bitmex.Client.Websocket.Responses.Margins.Margin;
using DeleteOrderRequest = TradeMarket.DataTransfering.Bitmex.Rest.Requests.DeleteOrderRequest;
using PlaceOrderRequest = TradeMarket.DataTransfering.Bitmex.Rest.Requests.Place.PlaceOrderRequest;
using AmmendOrderRequest = TradeMarket.DataTransfering.Bitmex.Rest.Requests.Ammend.AmmendOrderRequest;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Wallets;
using Bitmex.Client.Websocket.Responses.Instruments;
using TradeMarket.Model.Publishers;
using TradeMarket.Model.UserContexts;

namespace TradeMarket.DataTransfering.Bitmex.Model
{
    public class BitmexTradeMarket : TradeMarket.Model.TradeMarkets.TradeMarket
    {
        public BitmexTradeMarket() : base() { }

        public BitmexTradeMarket(string name, IPublisherFactory factory) : base(name, factory) { }

      
        #region SubscribeRequests


        public async override void SubscribeToInstruments(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler, UserContext context)
        {
            if (InstrumentPublisher is null)
            {
                InstrumentPublisher = PublisherFactory.CreateInstrumentPublisher(CommonWSClient, context.SlotName);
                InstrumentPublisher.Changed += handler;
            }
            await InstrumentPublisher.Start();
        }


        public async override void SubscribeToUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler, UserContext context)
        {
            if (PositionPublisher[context] is null)
            {
                PositionPublisher[context] = PublisherFactory.CreateUserPositionPublisher(context);
                PositionPublisher[context].Changed += handler;
            }
            await PositionPublisher[context].Start();
        }

        public async override void SubscribeToUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler, UserContext context)
        {
            if (MarginPublisher[context] is null)
            {
                MarginPublisher[context] = PublisherFactory.CreateUserMarginPublisher(context);
                MarginPublisher[context].Changed += handler;
            }
            await MarginPublisher[context].Start();
        }

        public async override void SubscribeToBook25(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler, UserContext context)
        {
            if(Book25Publisher is null)
            {
                Book25Publisher = PublisherFactory.CreateBook25Publisher(CommonWSClient, context.SlotName);
                Book25Publisher.Changed += handler;
            }
            await Book25Publisher.Start();
        }

        public async override void SubscribeToUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler, UserContext context)
        {
            if (OrderPublisher[context] is null)
            {
                OrderPublisher[context] = PublisherFactory.CreateUserOrderPublisher(context);
                OrderPublisher[context].Changed += handler;
            }
            await OrderPublisher[context].Start();
        }

        public async override void SubscribeToBalance(EventHandler<IPublisher<Wallet>.ChangedEventArgs> handler, UserContext context)
        {
            if (WalletPublishers[context] is null)
            {
                WalletPublishers[context] = PublisherFactory.CreateWalletPublisher(context);
                WalletPublishers[context].Changed += handler;
            }
            await WalletPublishers[context].Start();
        }
        #endregion

        #region RestREquests
        public async override Task<DefaultResponse> AmmendOrder(string id, double? price, long? Quantity, long? LeavesQuantity, UserContext context)
        {
            var response = await CommonRestClient.SendAsync(new AmmendOrderRequest(context.Key, context.Secret, new() {Id = id, LeavesQuantity = LeavesQuantity, Price = price, Quantity = Quantity }), new System.Threading.CancellationToken());
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
                Log.Error("Id {0}, quantity {1}, price {2}, leavesQty {3}", id, Quantity, price, LeavesQuantity);
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
            var response = await CommonRestClient.SendAsync(new DeleteOrderRequest(context.Key, context.Secret, id), new System.Threading.CancellationToken());
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
            var response = await CommonRestClient.SendAsync(new PlaceOrderRequest(context.Key, context.Secret, new Order
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

        public async override Task<DefaultResponse> AutheticateUser(UserContext context)
        {
            if (AuthenticationPublisher[context] is null)
            {
                AuthenticationPublisher[context] = PublisherFactory.CreateAuthenticationPublisher(context);
            }
            //TODO нужно ожидать пока придет ответ от биржи
            bool answer = false;
            AuthenticationPublisher[context].Changed += (sender, args) => { answer = args.Changed; };

            await AuthenticationPublisher[context].Start();
            return new DefaultResponse
            {
                Code = answer ? ReplyCode.Succeed : ReplyCode.Failure,
                Message = ""
            };
        }
    }
}
