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
        public FormerClient(string uri)
        {
            _client = new FormerService.FormerServiceClient(GrpcChannel.ForAddress(uri));
        }

        public async Task UpdateConfig(TradeBot.Common.v1.UpdateServerConfigRequest update, Metadata meta)
        {
            while (true)
            {
                try
                {
                    await _client.UpdateServerConfigAsync(new TradeBot.Former.FormerService.v1.UpdateServerConfigRequest
                    {
                        Request = update
                    }, meta);
                    break;

                }
                catch (Exception e)
                {
                    Log.Information("{@Where}: Exception {@Exception}", "Relay",e.Message);
                    throw;
                }
            }
        }

        public Task<TradeBot.Relay.RelayService.v1.DeleteOrderResponse> SendDeleteOrder(TradeBot.Relay.RelayService.v1.DeleteOrderRequest request, ServerCallContext context)
        {
            while (true)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested) break;
                    var response = _client.DeleteOrder(new TradeBot.Former.FormerService.v1.DeleteOrderRequest {},context.RequestHeaders);
                    Log.Information("{@Where}: {@MethodName} \n args: request={@request}", "Relay", new System.Diagnostics.StackFrame().GetMethod().Name, request);
                    Log.Information("{@Where}: {@MethodName} \n args: response={@response}", "Relay", new System.Diagnostics.StackFrame().GetMethod().Name, response);
                    return Task.FromResult(new TradeBot.Relay.RelayService.v1.DeleteOrderResponse{});
                }
                catch (RpcException e)
                {
                    Log.Error("{@Where}: Exception" + e.Message, "Facade");
                }
            }
            Log.Information("{@Where}: Client disconnected", "Facade");
            return Task.FromResult(new TradeBot.Relay.RelayService.v1.DeleteOrderResponse { });
        }

    }
}

