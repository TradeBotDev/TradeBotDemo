using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.DataTransfering;
using TradeMarket.Former;

namespace TradeMarket.Services
{
    public class OrderFormerSerice : TradeMarket.Former.OrderFormerService.OrderFormerServiceBase
    {
        private Transferrer _transferrer;

        private List<SubscribeOrdersReply> _replies;

        public  OrderFormerSerice(Transferrer transferrer)
        {
            _replies = new List<SubscribeOrdersReply>();
            _transferrer = transferrer;
            _transferrer.ordersChanged += _transferrer_ordersChanged;
        }

        private void _transferrer_ordersChanged(object sender, OrdersAddedEventArgs args)
        {
            //TODO Тут должен быть конвертер а не вот так вот
            _replies.AddRange(args.addedOrders.Select(el => new SubscribeOrdersReply
            {
                SimpleOrderInfo = new TradeBot.Common.Order
                {
                    LastUpdateDate = new Timestamp()
                    {
                        Seconds = el.LastUpdateDate.Second
                    },
                     Price = el.Price,
                     Quantity = el.Quantity,
                     Signature = el.Signature,
                },
                RemoveDate = new Timestamp()
                {
                    Seconds = el.RemoveDate.Second
                },
                Id = el.Id,
                RemovePrice =el.RemovePrice
            }));
        }

        public override Task<BuyOrderReply> BuyOrder(BuyOrderRequest request, ServerCallContext context)
        {
            return Task.FromResult(new BuyOrderReply
            {
                Reply = new TradeBot.Common.DefaultReply
                {
                    Code = TradeBot.Common.ReplyCode.Succeed,
                    Message = "Order placed"
                }
            }) ;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override async Task SubscribeOrders(SubscribeOrdersRequest request, IServerStreamWriter<SubscribeOrdersReply> responseStream, ServerCallContext context)
        {
            
            foreach (var order in _replies)
            {
                await responseStream.WriteAsync(order);
            }
        }
    }
}
