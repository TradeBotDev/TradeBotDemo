using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Instruments;
using Bitmex.Client.Websocket.Responses.Margins;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChangesType = TradeBot.TradeMarket.TradeMarketService.v1.ChangesType;

namespace TradeMarket.Services
{
    public static class ConvertService
    {
        #region Responses
        public static TradeBot.TradeMarket.TradeMarketService.v1.SubscribeMarginResponse ConvertMargin(Margin margin,BitmexAction action)
        {
            return new()
            {
                ChangedType = ConvertChangeType(action),
                Margin = new()
                {
                    AvailableMargin = margin.AvailableMargin ?? default(long),
                    RealisedPnl = margin.RealisedPnl ?? default(long),
                    MarginBalance = margin.MarginBalance ?? default(long)
                }
            };
        }

        public static TradeBot.TradeMarket.TradeMarketService.v1.SubscribeMyOrdersResponse ConvertMyOrder(Order order, BitmexAction action)
        {
            return new()
            {
                Response = new()
                {
                    Code = string.IsNullOrEmpty(order.OrdRejReason) ? TradeBot.Common.v1.ReplyCode.Succeed : TradeBot.Common.v1.ReplyCode.Failure,
                    Message = order.OrdRejReason ?? ""
                },
                Changed = Convert(order, action),
                ChangesType = ConvertChangeType(action) 
            };
        }
        
        public static TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersResponse ConvertBookOrders(BookLevel book,BitmexAction action)
        {
            return new()
            {
                ChangedType = ConvertChangeType(action),
                Response = new()
                {
                   Order = Convert(book,action)
                }
            };
        }
        
        public static TradeBot.TradeMarket.TradeMarketService.v1.SubscribePositionResponse ConvertPosition(Position position,BitmexAction action)
        {
            return new()
            {
                CurrentQty = position.CurrentQty ?? default(long)
            };
        }

        public static TradeBot.TradeMarket.TradeMarketService.v1.SubscribePriceResponse ConvertInstrument(Instrument instrument,BitmexAction action)
        {
            return new()
            {
                AskPrice = instrument.AskPrice ?? default(double),
                BidPrice = instrument.BidPrice ?? default(double),
                FairPrice = instrument.FairPrice ?? default(double),
                ChangedType = ConvertChangeType(action)
            };
        }
        
        
        #endregion

        public static ChangesType ConvertChangeType(BitmexAction action)
        {
            switch (action)
            {
                case BitmexAction.Partial: return ChangesType.Partitial;
                case BitmexAction.Insert: return ChangesType.Insert;
                case BitmexAction.Update: return ChangesType.Update;
                case BitmexAction.Delete: return ChangesType.Delete;
            }
            return ChangesType.Undefiend;
        }

        public static TradeBot.Common.v1.Order Convert(BookLevel book, BitmexAction action)
        {
            return new()
            {
                Signature = ConvertSignature(book,action),
                Id = book.Id.ToString(),
                LastUpdateDate = new Google.Protobuf.WellKnownTypes.Timestamp()
                {
                    Seconds = DateTime.Now.Second
                },
                Price = book.Price.HasValue ? book.Price.Value : default(double),
                Quantity = book.Size.HasValue ? (int)book.Size.Value : default(int)
            };
        }

        public static TradeBot.Common.v1.Order Convert(Order order, BitmexAction action)
        {
            return new()
            {
                Signature = ConvertSignature(order,action),
                Id = order.OrderId.ToString(),
                LastUpdateDate = new Google.Protobuf.WellKnownTypes.Timestamp()
                {
                    Seconds = order.Timestamp.HasValue ? order.Timestamp.Value.Second : DateTime.Now.Second
                },
                Price = order.Price.HasValue ? order.Price.Value : default(double),
                Quantity = order.OrderQty.HasValue ? (int)order.OrderQty.Value : default(int)
            };
        }

        public static TradeBot.Common.v1.OrderSignature ConvertSignature(Order order,BitmexAction action)
        {
            return new()
            {
                Status = ConvertOrderStatus(action),
                Type = ConvertOrderType(order)
            };
        }

        public static TradeBot.Common.v1.OrderSignature ConvertSignature(BookLevel book, BitmexAction action)
        {
            return new()
            {
                Status = ConvertOrderStatus(action),
                Type = ConvertOrderType(book)
            };
        }

        public static TradeBot.Common.v1.OrderType ConvertOrderType(Order order)
        {
            return order.Side == BitmexSide.Buy ? TradeBot.Common.v1.OrderType.Buy : TradeBot.Common.v1.OrderType.Sell;
        }

        public static TradeBot.Common.v1.OrderType ConvertOrderType(BookLevel book)
        {
            return book.Side == BitmexSide.Buy ? TradeBot.Common.v1.OrderType.Buy : TradeBot.Common.v1.OrderType.Sell;
        }

        public static TradeBot.Common.v1.OrderStatus ConvertOrderStatus(BitmexAction action)
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

        public static TradeBot.Common.v1.Balance ConvertBalance(Wallet wallet)
        {
            return new()
            {
                Currency = wallet.Currency,
                Value = wallet.BalanceBtc.ToString()
            };
        }


        
    }
}
