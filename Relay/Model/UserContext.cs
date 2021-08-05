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
        private bool IsWorking = false;

        public UserContext(Metadata meta, FormerClient formerClient, AlgorithmClient algorithmClient, TradeMarketClient tradeMarketClient)
        {
            Meta = meta;
            _formerClient = formerClient;
            _algorithmClient = algorithmClient;
            _tradeMarketClient = tradeMarketClient;

            _algorithmStream = _algorithmClient.OpenStream(meta);
            _tradeMarketStream = _tradeMarketClient.OpenStream(meta);
            
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

        public IAsyncStreamReader<SubscribeOrdersResponse> ReConnect()
        {
            _tradeMarketStream = _tradeMarketClient.OpenStream(Meta);
            return _tradeMarketStream;
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
