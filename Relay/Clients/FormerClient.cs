using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.Former.FormerService.v1;

namespace Relay.Clients
{
    public class FormerClient
    {
        private readonly FormerService.FormerServiceClient _client;
        public FormerClient(Uri uri)
        {
            _client = new FormerService.FormerServiceClient(GrpcChannel.ForAddress(uri));
        }

        public async Task UpdateConfig(Config config,Metadata meta)
        {
             await _client.UpdateServerConfigAsync(new() {Request = config }, meta);
        }

        public IAsyncStreamReader<TradeBot.Former.FormerService.v1.SubscribeLogsResponse> OpenStream()
        {
            return _client.SubscribeLogs(new TradeBot.Former.FormerService.v1.SubscribeLogsRequest 
            { 
                Request=new TradeBot.Common.v1.SubscribeLogsRequest 
                {
                    Level=LogLevel.None 
                } 
            }).ResponseStream;
        }
        public IAsyncEnumerable<TradeBot.Former.FormerService.v1.SubscribeLogsResponse> SubscribeForLogs(IAsyncStreamReader<TradeBot.Former.FormerService.v1.SubscribeLogsResponse> stream)
        {
            var channel = System.Threading.Channels.Channel.CreateUnbounded<TradeBot.Former.FormerService.v1.SubscribeLogsResponse>();
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        while (await stream.MoveNext())
                        {
                            await channel.Writer.WriteAsync(new TradeBot.Former.FormerService.v1.SubscribeLogsResponse { Response=new TradeBot.Common.v1.SubscribeLogsResponse { Level=LogLevel.None} });
                        }
                        break;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.Message);
                    }
                }
            });
            return channel.Reader.ReadAllAsync();

        }

    }
}

