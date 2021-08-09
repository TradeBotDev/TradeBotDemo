using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.Former.FormerService.v1;
using TradeBot.Relay.RelayService.v1;

namespace Relay.Clients
{
    public class FormerClient
    {
        private readonly FormerService.FormerServiceClient _client;
        public FormerClient(Uri uri)
        {
            _client = new FormerService.FormerServiceClient(GrpcChannel.ForAddress(uri));
        }

        public async Task UpdateConfig(TradeBot.Common.v1.UpdateServerConfigRequest update, Metadata meta)
        {
             await _client.UpdateServerConfigAsync(new TradeBot.Former.FormerService.v1.UpdateServerConfigRequest { Request=update}, meta);
        }

        public Task<TradeBot.Relay.RelayService.v1.DeleteOrderResponse> SendDeleteOrder(TradeBot.Relay.RelayService.v1.DeleteOrderRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;
                    var response = _client.DeleteOrder(new TradeBot.Former.FormerService.v1.DeleteOrderRequest {});
                    Log.Information("{@Where}: {@MethodName} \n args: request={@request}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, request);
                    Log.Information("{@Where}: {@MethodName} \n args: response={@response}", "Facade", new System.Diagnostics.StackFrame().GetMethod().Name, response);
                    return Task.FromResult(new TradeBot.Relay.RelayService.v1.DeleteOrderResponse
                    {
                        Response = new TradeBot.Common.v1.DefaultResponse
                        {
                            Code = response.Response.Code,
                            Message = response.Response.Message
                        }
                    }
                    );
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception" + e.Message, "Facade");
                }
            }
            Log.Information("{@Where}: Client disconnected", "Facade");
            return Task.FromResult(new TradeBot.Relay.RelayService.v1.DeleteOrderResponse { });
        }
        public IAsyncStreamReader<TradeBot.Former.FormerService.v1.SubscribeLogsResponse> OpenStream(Metadata meta)
        {
            return _client.SubscribeLogs(new TradeBot.Former.FormerService.v1.SubscribeLogsRequest 
            { 
                Request=new TradeBot.Common.v1.SubscribeLogsRequest 
                {
                    Level=LogLevel.None 
                } 
            }, meta).ResponseStream;
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
                            await channel.Writer.WriteAsync(new TradeBot.Former.FormerService.v1.SubscribeLogsResponse 
                            { 
                                Response=new TradeBot.Common.v1.SubscribeLogsResponse 
                                { 
                                    LogMessage=stream.Current.Response.LogMessage
                                } 
                            });
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

