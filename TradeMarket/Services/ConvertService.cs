using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Instruments;
using Bitmex.Client.Websocket.Responses.Margins;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeMarket.DataTransfering.Bitmex.Rest.Responses;
using Order = Bitmex.Client.Websocket.Responses.Orders.Order;

namespace TradeMarket.Services
{
    public static class ConvertService
    {
        #region Service to Responses Converters

        public static TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceResponse ConvertBalance(Wallet wallet, BitmexAction action)
        {
            return new()
            {
                Response = new()
                {
                    Balance = ConvertBalanceFromWallet(wallet)
                }
            };
        }

        public static TradeBot.TradeMarket.TradeMarketService.v1.SubscribeMarginResponse ConvertMargin(Margin margin,BitmexAction action)
        {
            return new()
            {
                ChangedType = ConvertChangeType(action),
                Margin = new()
                {
                    AvailableMargin = margin.AvailableMargin ?? default,
                    RealisedPnl = margin.RealisedPnl ?? default,
                    MarginBalance = margin.MarginBalance ?? default
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
                CurrentQty = position.CurrentQty ?? default
            };
        }

        public static TradeBot.TradeMarket.TradeMarketService.v1.SubscribePriceResponse ConvertInstrument(Instrument instrument,BitmexAction action)
        {
            return new()
            {
                AskPrice = instrument.AskPrice ?? default,
                BidPrice = instrument.BidPrice ?? default,
                FairPrice = instrument.FairPrice ?? default,
                 LotSize = instrument.LotSize is null ? default : (int) instrument.LotSize,
                ChangedType = ConvertChangeType(action)
            };
        }


        #endregion

        #region Bitmex to Service Converters
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
                Price = book.Price.HasValue ? book.Price.Value : default,
                Quantity = book.Size.HasValue ? (int)book.Size.Value : default
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
                    Seconds = order.Timestamp?.Second ?? DateTime.Now.Second
                },
                Price = order.Price ?? default,
                Quantity = order.OrderQty.HasValue ? (int)order.OrderQty.Value : default
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
            switch (order.Side)
            {
                case BitmexSide.Undefined:
                    return OrderType.Unspecified;
                    break;
                case BitmexSide.Buy:
                    return OrderType.Buy;
                    break;
                case BitmexSide.Sell:
                    return OrderType.Sell;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        public static TradeBot.Common.v1.Balance ConvertBalanceFromWallet(Wallet wallet)
        {
            return new()
            {
                Currency = wallet.Currency,
                Value = wallet.BalanceBtc.ToString()
            };
        }

        #endregion

        #region Commands to Response Converters
        public static TradeBot.TradeMarket.TradeMarketService.v1.PlaceOrderResponse ConvertPlaceOrderResponse(BitmexResfulResponse<Order> response)
        {
            return new()
            {
                OrderId = response.Error is not null ? "empty" : response.Message.OrderId,
                Response = ResponseFromOrder(response)
            };
        }

        public static TradeBot.TradeMarket.TradeMarketService.v1.AmmendOrderResponse ConvertAmmendOrderResponse(BitmexResfulResponse<Order> response)
        {
            return new()
            {
                Response = ResponseFromOrder(response)
            };
        }

        public static TradeBot.TradeMarket.TradeMarketService.v1.DeleteOrderResponse ConvertDeleteOrderResponse(BitmexResfulResponse<Order[]> response)
        {
            return new()
            {
                Response = ResponseFromOrder(response)
            };
        }

        public static DefaultResponse ResponseFromOrder(BitmexResfulResponse<Order> response)
        {
            if (response.Error is not null)
            {
                return new DefaultResponse()
                {
                    Code = ReplyCode.Failure,
                    Message = response.Error.Message
                };
            }
            if (!string.IsNullOrEmpty(response.Message.OrdRejReason))
            {
                return new DefaultResponse()
                {
                    Code = ReplyCode.Failure,
                    Message = response.Message.OrdRejReason
                };
            }
            return new DefaultResponse()
            {
                Code = ReplyCode.Succeed,
                Message = ""
            };
        }

        public static DefaultResponse ResponseFromOrder(BitmexResfulResponse<Order[]> response)
        {
            if (response.Error is not null)
            {
                return new DefaultResponse()
                {
                    Code = ReplyCode.Failure,
                    Message = response.Error.Message
                };
            }
            if (!string.IsNullOrEmpty(response.Message[0].OrdRejReason))
            {
                return new DefaultResponse()
                {
                    Code = ReplyCode.Failure,
                    Message = response.Message[0].OrdRejReason
                };
            }
            return new DefaultResponse()
            {
                Code = ReplyCode.Succeed,
                Message = ""
            };
        }
        #endregion


    }
}
