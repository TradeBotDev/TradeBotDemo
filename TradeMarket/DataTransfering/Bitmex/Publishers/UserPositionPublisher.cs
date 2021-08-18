﻿using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Responses.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Model.Publishers;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class UserPositionPublisher : BitmexPublisher<PositionResponse,PositionSubscribeRequest,Position>
    {
        internal static readonly Action<PositionResponse, EventHandler<IPublisher<Position>.ChangedEventArgs>> _action = async (response, e) =>
        {
           await Task.Run(() =>
           {
               foreach (var data in response.Data)
               {
                   e?.Invoke(nameof(UserOrderPublisher), new(data, response.Action));
               }
           });
        };

        private IObservable<PositionResponse> _stream;

        #region Parameters For SubscribeAsync

        private CancellationToken _token;
        #endregion

        public UserPositionPublisher(BitmexWebsocketClient client, IObservable<PositionResponse> stream,CancellationToken token) : base(client, _action)
        {
            _stream = stream;

            _token = token;
        }

        public override void AddModelToCache(PositionResponse response)
        {
            lock (locker)
            {
                foreach(var el in response.Data)
                {
                    var model = _cache.FirstOrDefault(x => x.Symbol == el.Symbol);
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
                                    el.CurrentQty = el.CurrentQty is null || el.CurrentQty == 0 ? model.CurrentQty : el.CurrentQty;
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

        public async override Task Start()
        {
            await SubscribeAsync(_token);
        }

        public async Task SubscribeAsync(CancellationToken token)
        {
            await base.SubscribeAsync(new PositionSubscribeRequest(), _stream, token);

        }
    }
}
