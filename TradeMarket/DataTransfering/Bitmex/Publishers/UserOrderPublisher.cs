using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Responses.Orders;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Model.Publishers;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class UserOrderPublisher : BitmexPublisher<OrderResponse,OrderSubscribeRequest,Order>
    {
        internal static readonly Action<OrderResponse, EventHandler<IPublisher<Order>.ChangedEventArgs>> _action = async (response, e) =>
        {
            await Task.Run(() =>
           {
               foreach (var data in response.Data)
               {
                    //при исполнении ордера с биржи прилетает не делит а апдейт
                    BitmexAction action = response.Action;
                   if (data.Price is null && data.OrderQty is null)
                   {
                       action = BitmexAction.Delete;
                   }
                   //Log.Information("{@Where} {@OrderId} {@OrderQuantity} @{OrderPrice} @{OrderAction}", "Trademarket", data.OrderId, data.OrderQty, data.Price, action);
                   e?.Invoke(typeof(UserOrderPublisher), new(data, action));
               }
           });
        };

        private IObservable<OrderResponse> _stream;
        private readonly CancellationToken _token;

        public UserOrderPublisher(BitmexWebsocketClient client,IObservable<OrderResponse> stream,CancellationToken token) : base(client,_action)
        {
            _stream = stream;
            this._token = token;
        }

        public async override Task Start()
        {
            await SubscribeAsync(_token);
        }

        public async Task SubscribeAsync(CancellationToken token)
        {
            await base.SubscribeAsync(new OrderSubscribeRequest(), _stream, token);
        }

        public override void AddModelToCache(OrderResponse response)
        {
            lock (locker)
            {
                Parallel.ForEach(response.Data, (el) =>
                {
                    var model = _cache.FirstOrDefault(x => x.OrderId == el.OrderId);
                    if (model is not null)
                    {
                        switch (response.Action)
                        {
                            case BitmexAction.Delete:
                                {
                                    _cache.Remove(model);
                                    break;
                                }
                            case BitmexAction.Update:
                                {
                                    _cache.Remove(model);
                                    el.Price = el.Price is null || el.Price == 0 ? model.Price : el.Price;
                                    el.OrderQty = el.OrderQty is null || el.OrderQty == 0 ? model.OrderQty : el.OrderQty;
                                    _cache.Add(el);
                                    break;
                                }
                            default:
                                {
                                    _cache.Add(el);
                                    break;
                                }
                        }
                    }
                    else
                    {
                        _cache.Add(el);
                    }

                });
            }
        }
    }
}
