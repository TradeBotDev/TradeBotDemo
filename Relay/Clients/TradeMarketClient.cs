﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Relay.Model;
using Serilog;
using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;
using SubscribeOrdersRequest = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersRequest;
using SubscribeOrdersResponse = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersResponse;

namespace Relay.Clients
{
    public class TradeMarketClient
    {
        private TradeMarketService.TradeMarketServiceClient _client;
        private CancellationTokenSource _token;

        public TradeMarketClient(string uri)
        {
            _client = new TradeMarketService.TradeMarketServiceClient(GrpcChannel.ForAddress(uri));
        }

        public IAsyncStreamReader<SubscribeOrdersResponse> OpenStream(Metadata meta)
        {
            var response = _client.SubscribeOrders(new SubscribeOrdersRequest()
            {
                Request = new TradeBot.Common.v1.SubscribeOrdersRequest()
                {
                    Signature = new OrderSignature
                    {
                        Type = OrderType.Unspecified,
                        Status = OrderStatus.Unspecified
                    }
                }
            }, meta);
            return response.ResponseStream;
        }
        public IAsyncStreamReader<SubscribeOrdersResponse> ReConnect(Metadata Meta)
        {
            return OpenStream(Meta);
        }


        public IAsyncEnumerable<Order> SubscribeForOrders(IAsyncStreamReader<SubscribeOrdersResponse> stream,Metadata Meta)
        {
            _token = new CancellationTokenSource();
            System.Threading.Channels.Channel<Order> channel = System.Threading.Channels.Channel.CreateUnbounded<Order>();
            Task.Run(async() =>
            {
                while (true)
                {
                    try
                    {
                        while (await stream.MoveNext(_token.Token))
                        {
                            await channel.Writer.WriteAsync(new(stream.Current.Response.Order));
                            OrderRecievedEvent?.Invoke(this, new(stream.Current.Response.Order));
                        }
                        break;
                    }
                    catch (RpcException e)
                    {
                        Log.ForContext("sessionId", Meta.GetValue("sessionid")).ForContext("slot", Meta.GetValue("slot")).Information("{@Where}:Exception {@Exception}", "Relay",e.Message);
                        if (e.StatusCode==StatusCode.Cancelled) break;
                        await Task.Delay(5000);
                    }
                }
            });
            return channel.Reader.ReadAllAsync();

        }

        public void CancellationToken()
        {
            _token.Cancel();
        }
        public event EventHandler<Order> OrderRecievedEvent;
    }
}
