using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Margins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Model.Publishers;

namespace TradeMarket.DataTransfering.Bitmex.Publishers
{
    public class UserMarginPublisher : BitmexPublisher<MarginResponse,MarginSubscribeRequest, Margin>
    {
        internal static readonly Action<MarginResponse, EventHandler<IPublisher<Margin>.ChangedEventArgs>> _action = async (response, e) =>
        {
            await Task.Run(() =>
            {
                foreach (var data in response.Data)
                {
                    e?.Invoke(nameof(UserOrderPublisher), new(data, response.Action));
                }
            });
        };

        private IObservable<MarginResponse> _stream;
        private readonly CancellationToken _token;

        public UserMarginPublisher(BitmexWebsocketClient client, IObservable<MarginResponse> stream, CancellationToken token) : base(client, _action)
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
            await base.SubscribeAsync(new MarginSubscribeRequest(), _stream, token);

        }

        public override void AddModelToCache(MarginResponse response)
        {
            lock (locker)
            {
                foreach (var data in response.Data)
                {
                    var model = _cache.FirstOrDefault(x => x.Account == data.Account);
                    if(model is not null)
                    {
                        data.AvailableMargin = data.AvailableMargin is null || data.AvailableMargin == 0 ? model.AvailableMargin : data.AvailableMargin;
                        data.RealisedPnl = data.RealisedPnl is null || data.RealisedPnl == 0 ? model.RealisedPnl : data.RealisedPnl;
                        data.MarginBalance = data.MarginBalance is null || data.MarginBalance == 0 ? model.MarginBalance : data.MarginBalance;
                        _cache.Remove(model);
                    }
                    _cache.Add(data);
                }
            }
        }
    }
}
