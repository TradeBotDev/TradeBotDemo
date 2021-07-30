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

        private bool IsSubscribe { get; set; }
        public Metadata Meta{ get; internal set; }

        private IClientStreamWriter<AddOrderRequest> _algorithmStream;
        private IAsyncStreamReader<SubscribeOrdersResponse> _tradeMarketStream;


        public UserContext(Metadata meta, FormerClient formerClient, AlgorithmClient algorithmClient, TradeMarketClient tradeMarketClient)
        {
            Meta = meta;
            _formerClient = formerClient;
            _algorithmClient = algorithmClient;
            _tradeMarketClient = tradeMarketClient;

            _algorithmStream = _algorithmClient.OpenStream(meta);
            _tradeMarketStream = _tradeMarketClient.OpenStream(meta);

            //_tradeMarketClient.OrderRecievedEvent += _tradeMarketClient_OrderRecievedEvent;
            IsSubscribe = false;
        }

        public void StatusOfSubscribe()
        {
            if(!IsSubscribe)
            {
                _tradeMarketClient.OrderRecievedEvent += _tradeMarketClient_OrderRecievedEvent;
                IsSubscribe = true;
                Log.Information("The bot is starting...");
            }
            else
            {
                _tradeMarketClient.OrderRecievedEvent -= _tradeMarketClient_OrderRecievedEvent;
                IsSubscribe = false;
                Log.Information("The bot is stopping...");
            }
        }
        public static async Task<TradeBot.Relay.RelayService.v1.StartBotResponse> ReturnBotStatus(UserContext context)
        {
            if (context.IsSubscribe)
            {
                return await Task.FromResult(new TradeBot.Relay.RelayService.v1.StartBotResponse()
                {
                    Response = new DefaultResponse()
                    {
                        Message = "Bot was launched",
                        Code = ReplyCode.Succeed
                    }
                });
            }
            else
            {
                return await Task.FromResult(new TradeBot.Relay.RelayService.v1.StartBotResponse()
                {
                    Response = new DefaultResponse()
                    {
                        Message = "Bot was stoped",
                        Code = ReplyCode.Succeed
                    }
                });
            }
        }
        private void _tradeMarketClient_OrderRecievedEvent(object sender, TradeBot.Common.v1.Order e)
        {
            Log.Information($"Sending order {e.Price} : {e.Quantity} : {e.Id}");
            _algorithmClient.WriteOrder(_algorithmStream,e);
        }

        public void UpdateConfig(Config config)
        {
            _ = _algorithmClient.UpdateConfig(config, Meta);
            _ = _formerClient.UpdateConfig(config, Meta);
        }

        public void SubscribeForOrders()
        {
            if(IsSubscribe) _tradeMarketClient.SubscribeForOrders(_tradeMarketStream);
        }

    }
}
