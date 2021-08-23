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
        private bool IsWorking = true;

        public UserContext(Metadata meta, FormerClient formerClient, AlgorithmClient algorithmClient, TradeMarketClient tradeMarketClient)
        {
            Meta = meta;
            _formerClient = formerClient;
            _algorithmClient = algorithmClient;
            _tradeMarketClient = tradeMarketClient;
            
            _algorithmStream = _algorithmClient.OpenStream(meta);
            _tradeMarketStream = _tradeMarketClient.OpenStream(meta);
            _tradeMarketClient.OrderRecievedEvent += _tradeMarketClient_OrderRecievedEvent;
        }
        public IAsyncStreamReader<SubscribeOrdersResponse> ReConnect()
        {
            _tradeMarketStream = _tradeMarketClient.OpenStream(Meta);
            return _tradeMarketStream;
        }

        public void StatusOfWork()
        {
            if (!IsWorking)
            {
                IsWorking = true;
                //_tradeMarketClient.OrderRecievedEvent -= _tradeMarketClient_OrderRecievedEvent;
                _tradeMarketClient.CancellationToken();
                Log.Information("The bot is stopping...");
            }
            else
            {
                IsWorking = false;
                //прокинуть openstream
                //_tradeMarketClient.OrderRecievedEvent += _tradeMarketClient_OrderRecievedEvent;
                ReConnect();
                Log.Information("The bot is starting...");
            }
        }


        private void _tradeMarketClient_OrderRecievedEvent(object sender, TradeBot.Common.v1.Order e)
        {
            Log.Information("{@Where}: Sending order Price={@Price} : Quantity={@Quantity} : Id={@Id}", "Relay",e.Price,e.Quantity,e.Id);
            Task.Run(async()=> 
            { 
                await _algorithmClient.WriteOrder(_algorithmStream, e);
            }).Wait();
        }

        public void UpdateConfig(TradeBot.Common.v1.UpdateServerConfigRequest update)
        {
            _ = _algorithmClient.UpdateConfig(update, Meta);
            _ = _formerClient.UpdateConfig(update, Meta);
        }

        public async Task SubscribeForOrders()
        {
            
            if (!IsWorking && !IsStart)
            {
                IsStart = IsStart ? !IsStart : IsStart;
                _tradeMarketClient.SubscribeForOrders(_tradeMarketStream);
            }
        }


    }
}
