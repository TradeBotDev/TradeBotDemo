using Grpc.Core;
using Relay.Clients;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Algorithm.AlgorithmService.v1;
using TradeBot.Common.v1;
using SubscribeOrdersResponse = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersResponse;

namespace Relay.Model
{
    public class UserContext
    {
        public FormerClient _formerClient { get; internal set; }
        public AlgorithmClient _algorithmClient { get; internal set; }
        public TradeMarketClient _tradeMarketClient { get; internal set; }

        private bool IsStart = false;
        public Metadata Meta { get; internal set; }

        private IClientStreamWriter<AddOrderRequest> _algorithmStream;
        private IAsyncStreamReader<SubscribeOrdersResponse> _tradeMarketStream;
        private IAsyncStreamReader<TradeBot.Former.FormerService.v1.SubscribeLogsResponse> _formerStream;
        private bool IsWorking = false;

        public UserContext(Metadata meta, FormerClient formerClient, AlgorithmClient algorithmClient, TradeMarketClient tradeMarketClient)
        {
            Meta = meta;
            _formerClient = formerClient;
            _algorithmClient = algorithmClient;
            _tradeMarketClient = tradeMarketClient;
            
            _algorithmStream = _algorithmClient.OpenStream(meta);
            _tradeMarketStream = _tradeMarketClient.OpenStream(meta);
            _formerStream = _formerClient.OpenStream(meta);//мб нужно будет кидать мету
        }
        public IAsyncStreamReader<SubscribeOrdersResponse> ReConnect()
        {
            _tradeMarketStream = _tradeMarketClient.OpenStream(Meta);
            return _tradeMarketStream;
        }

        public void StatusOfWork()
        {
            if (IsWorking)
            {
                IsWorking = false;
                _tradeMarketClient.OrderRecievedEvent -= _tradeMarketClient_OrderRecievedEvent;
                Log.Information("The bot is stopping...");
            }
            else
            {
                IsWorking = true;
                _tradeMarketClient.OrderRecievedEvent += _tradeMarketClient_OrderRecievedEvent;
                Log.Information("The bot is starting...");
            }
        }


        private void _tradeMarketClient_OrderRecievedEvent(object sender, TradeBot.Common.v1.Order e)
        {
            Log.Information($"Sending order {e.Price} : {e.Quantity} : {e.Id}");
            Task.Run(async()=> 
            { 
                await _algorithmClient.WriteOrder(_algorithmStream, e);
            }).Wait();
        }

        public void UpdateConfig(Config config)
        {
            _ = _algorithmClient.UpdateConfig(config, Meta);
            _ = _formerClient.UpdateConfig(config, Meta);
        }
        
        public async void RepeatLogsFormer(TradeBot.Relay.RelayService.v1.SubscribeLogsRequest request, IServerStreamWriter<TradeBot.Relay.RelayService.v1.SubscribeLogsResponse> responseStream)
        {
            await foreach (var item in _formerClient.SubscribeForLogs(_formerStream))
            {
                try {
                    await responseStream.WriteAsync(new TradeBot.Relay.RelayService.v1.SubscribeLogsResponse { Response = item.Response });
                }
                catch(Exception e)
                {
                    Log.Error(e.Message);
                }
            }

        }



        public void SubscribeForOrders()
        {
            if (IsWorking && !IsStart)
            {
                IsStart = true;
                _tradeMarketClient.SubscribeForOrders(_tradeMarketStream);
            }
        }

    }
}
