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
        internal static readonly Action<OrderResponse, EventHandler<IPublisher<Order>.ChangedEventArgs>,ILogger> _action = async (response, e,logger) =>
        {
            await Task.Run(() =>
           {
               var log = logger.ForContext<UserOrderPublisher>();
               try
               {
                   foreach (var data in response.Data)
                   {
                       //при исполнении ордера с биржи прилетает не делит а апдейт и показывается что ордер "перестал работать"
                       BitmexAction action = response.Action;
                       if ((data.Price is null && data.OrderQty is null) || (data.WorkingIndicator is not null && data.WorkingIndicator == false))
                       {
                           action = BitmexAction.Delete;
                       }
                        log.Information("Response : {@Response}", data);
                        log.Information("User Order Recieved {@OrderId} {@OrderQuantity} @{OrderPrice} @{OrderAction}", data.OrderId, data.OrderQty, data.Price, action);
                        e?.Invoke(typeof(UserOrderPublisher), new(data, action));
                   }
               }catch(Exception e)
               {
                   log.Warning(e.Message);
                   log.Warning(e.StackTrace);
               }
           });
        };
        private OrderSubscribeRequest _request;
        private IObservable<OrderResponse> _stream;
        private readonly CancellationToken _token;

        public UserOrderPublisher(BitmexWebsocketClient client,IObservable<OrderResponse> stream, OrderSubscribeRequest orderSubscribeRequest, CancellationToken token) : base(client,_action)
        {
            _request = orderSubscribeRequest;
            _stream = stream;
            this._token = token;
        }

        public async override Task Start(ILogger logger)
        {
            var log = logger.ForContext<UserOrderPublisher>();
            await SubscribeAsync(_token,log);
        }

        public async Task SubscribeAsync(CancellationToken token,ILogger logger)
        {
            await base.SubscribeAsync(_request, _stream, token,logger);
        }

        public override void AddModelToCache(OrderResponse response)
        {
            lock (locker)
            {
                foreach(var el in response.Data)
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

                }
            }
        }

        public async override Task Stop(ILogger logger)
        {
            var log = logger.ForContext<UserOrderPublisher>();
            await UnSubscribeAsync(_request,logger);
        }
    }
}
