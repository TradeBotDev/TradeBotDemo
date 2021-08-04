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
using TradeMarket.DataTransfering.Bitmex.Rest.Responses;

namespace TradeMarket.DataTransfering.Bitmex.Model
{
    public class BitmexTradeMarket : TradeMarket.Model.TradeMarkets.TradeMarket
    {
        public BitmexTradeMarket() : base() { }

        public BitmexTradeMarket(string name, IPublisherFactory factory) : base(name, factory) { }
      
        #region SubscribeRequests
       

        public async override Task SubscribeToInstruments(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler, UserContext context)
        {
            if (InstrumentPublisher is null)
            {
                InstrumentPublisher = await CreatePublisher(handler, context, PublisherFactory.CreateInstrumentPublisher);
            }
            await InstrumentPublisher.Start();
        }


        public async override Task SubscribeToUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler, UserContext context)
        {
            if (PositionPublisher.ContainsKey(context) == false)
            {
                PositionPublisher[context] = await CreatePublisher(handler, context, PublisherFactory.CreateUserPositionPublisher);
            }
            await PositionPublisher[context].Start();
        }


        public async override Task SubscribeToUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler, UserContext context)
        {
            if (MarginPublisher.ContainsKey(context) == false)
            {
                MarginPublisher[context] = await CreatePublisher(handler, context, PublisherFactory.CreateUserMarginPublisher);
            }
            await MarginPublisher[context].Start();
        }

        public async override Task SubscribeToBook25(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler, UserContext context)
        {
            if(Book25Publisher is null)
            {
                Book25Publisher = await CreatePublisher(handler, context, PublisherFactory.CreateBook25Publisher);
            }
            await Book25Publisher.Start();
        }

        public async override Task SubscribeToUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler, UserContext context)
        {
            if (OrderPublisher.ContainsKey(context) == false)
            {
                OrderPublisher[context] = await CreatePublisher(handler, context, PublisherFactory.CreateUserOrderPublisher);
            }
            await OrderPublisher[context].Start();
        }

        public async override Task SubscribeToBalance(EventHandler<IPublisher<Wallet>.ChangedEventArgs> handler, UserContext context)
        {
            if (WalletPublishers.ContainsKey(context) == false)
            {
                WalletPublishers[context] = await CreatePublisher(handler, context, PublisherFactory.CreateWalletPublisher);
            }
            await WalletPublishers[context].Start();
        }
        #endregion

        #region RestRequests

        public Task<DefaultResponse> ResponseFromOrder(BitmexResfulResponse<Order> response)
        {
            if (response.Error is not null)
            {
                return Task.FromResult(new DefaultResponse()
                {
                    Code = ReplyCode.Failure,
                    Message = response.Error.Message
                });
            }
            if (!string.IsNullOrEmpty(response.Message.OrdRejReason))
            {
                return Task.FromResult(new DefaultResponse()
                {
                    Code = ReplyCode.Failure,
                    Message = response.Message.OrdRejReason
                });
            }
            return null;
        }

        public async override Task<DefaultResponse> AmmendOrder(string id, double? price, long? Quantity, long? LeavesQuantity, UserContext context)
        {
            var response = await CommonRestClient.SendAsync(new AmmendOrderRequest(context.Key, context.Secret, new() {Id = id, LeavesQuantity = LeavesQuantity, Price = price, Quantity = Quantity }), new System.Threading.CancellationToken());
            
            return await ResponseFromOrder(response) ?? new DefaultResponse()
            {
                Code = ReplyCode.Succeed,
                Message = $"Order {response.Message.OrderId} Ammended"
            };
        }
        public async override Task<DefaultResponse> DeleteOrder(string id, UserContext context)
        {
            var response = await CommonRestClient.SendAsync(new DeleteOrderRequest(context.Key, context.Secret, id), new System.Threading.CancellationToken());
            return await ResponseFromOrder(response) ?? new DefaultResponse()
            {
                Code = ReplyCode.Succeed,
                Message = $"Order {response.Message.OrderId} was deleted"
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
            return new()
            {
                OrderId = response.Message.OrderId,
                Response = await ResponseFromOrder(response) ?? new DefaultResponse()
                {
                    Code = ReplyCode.Succeed,
                    Message = $"Order Placed"
                }
            };
        }
        #endregion

        public async override Task<DefaultResponse> AutheticateUser(UserContext context)
        {
            bool answer = false;
            EventHandler<IPublisher<bool>.ChangedEventArgs> handler = (sender, args) => { answer = args.Changed; };
            if (AuthenticationPublisher.ContainsKey(context) == false)
            {
                AuthenticationPublisher[context] = await CreatePublisher(handler, context, PublisherFactory.CreateAuthenticationPublisher);
            }
            //TODO нужно ожидать пока придет ответ от биржи

            await AuthenticationPublisher[context].Start();
            return new DefaultResponse
            {
                Code = answer ? ReplyCode.Succeed : ReplyCode.Failure,
                Message = ""
            };
        }
    }
}
