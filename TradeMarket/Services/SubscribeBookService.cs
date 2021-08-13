using Grpc.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.TradeMarket.TradeMarketService.v1;

namespace TradeMarket.Services
{
    /// <summary>
    /// Подписка на стаканы биржи
    /// </summary>
    public partial class TradeMarketService : TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceBase
    {
        public async override Task SubscribeOrders(TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersRequest request, IServerStreamWriter<SubscribeOrdersResponse> responseStream, ServerCallContext context)
        { 
            
            try
            {
                var user = await TryGetUserContextFromMetadataAsync(context.RequestHeaders);
                user.Book25 += async (sender, args) => {
                    var response = ConvertService.ConvertBookOrders(args.Changed, args.Action);
                    if (IsOrderSuitForSignature(response.Response.Order.Signature, request.Request.Signature))
                    {
                        await WriteStreamAsync<SubscribeOrdersResponse>(responseStream, response);
                    }
                };
                //TODO отписка после отмены
                await AwaitCancellation(context.CancellationToken);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
                Log.Logger.Error(e.StackTrace);

                context.Status = Status.DefaultCancelled;
                context.RequestHeaders = this.AddInfoToMetadataAsync(user, context.RequestHeaders);

            }

        }
}
