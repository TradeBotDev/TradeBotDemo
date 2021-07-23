﻿using Grpc.Core;
using Relay.Clients;
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

            _tradeMarketClient.OrderRecievedEvent += _tradeMarketClient_OrderRecievedEvent;
        }

        private void _tradeMarketClient_OrderRecievedEvent(object sender, TradeBot.Common.v1.Order e)
        {
            _algorithmClient.WriteOrder(_algorithmStream,e);
        }

        public void UpdateConfig(Config config)
        {
            _ = _algorithmClient.UpdateConfig(config, Meta);
            _ = _formerClient.UpdateConfig(config, Meta);
        }

        public void SubscribeForOrders()
        {
            _tradeMarketClient.SubscribeForOrders(Meta);
        }

    }
}
