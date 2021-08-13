using Google.Protobuf;
using Grpc.Core;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using TradeBot.TradeMarket.TradeMarketService.v1;
using TradeMarket.Model.TradeMarkets;
using TradeMarket.Model.UserContexts;
using TradeMarket.Model.UserContexts.Builders;
using SubscribeBalanceRequest = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceRequest;
using SubscribeBalanceResponse = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceResponse;

namespace TradeMarket.Services
{
    public partial class TradeMarketService : TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceBase
    {
        private readonly ContextDirector _director;

        private readonly TradeMarketFactory _tradeMarketFactory;


        public TradeMarketService(ContextDirector director)
        {
            this._director = director;
        }

        /// <summary>
        /// Переводит заголовки запроса в язык сервиса и предоставляет контекст пользователя по переданым заголовкам
        /// </summary>
        public async Task<UserContext> GetUserContextAsync(Metadata meta)
        {
            return await Task.Run(async () =>
            {
                var sessionId = meta.Get("sessionid").Value;
                var slot = meta.Get("slot").Value;
                var trademarket = meta.Get("trademarket").Value;

                return await _director.GetUserContextAsync(sessionId, slot, trademarket);
            });
        }

        /// <summary>
        /// Предоставляет доступ к общему контексту биржи по слоту для доступа к информации для которой не нужно логирование
        /// </summary>
        public async Task<CommonContext> GetCommonContextAsync(Metadata meta)
        {
            return await Task.Run(async () =>
            {
                var slot = meta.Get("slot").Value;
                var trademarket = meta.Get("trademarket").Value;

                return await _director.GetCommonContextAsync(slot, trademarket);
            });
        }

        /// <summary>
        /// Метод заполняет заголовки для ответов по предоставленному контексту пользователя 
        /// </summary>
        public async Task<Metadata> AddInfoToMetadataAsync(IContext user, Metadata meta)
        {
            return await Task.Run(() =>
            {
                meta.Add("sessionid", user.Signature.SessionId);
                meta.Add("slot", user.Signature.SlotName);
                meta.Add("trademarket", user.Signature.TradeMarketName);
                return meta;
            });
        }

        /// <summary>
        /// Метод ожидает пока запрос не будет отменен по токену
        /// </summary>
        private static Task AwaitCancellation(CancellationToken token)
        {
            var completion = new TaskCompletionSource<object>();
            token.Register(() => completion.SetResult(null));
            return completion.Task;
        }


        /// <summary>
        /// Проверяет подходит ли принятый с биржи ордер для передачи его по запросу
        /// </summary>
        private static bool IsOrderSuitForSignature(TradeBot.Common.v1.OrderSignature orderSignature, TradeBot.Common.v1.OrderSignature signature)
        {
            bool typeCheck = false;
            bool statusCheck = false;
            if (signature.Status == TradeBot.Common.v1.OrderStatus.Unspecified || orderSignature.Status == signature.Status)
            {
                statusCheck = true;
            }
            if (signature.Type == TradeBot.Common.v1.OrderType.Unspecified || orderSignature.Type == signature.Type)
            {
                typeCheck = true;
            }
            return typeCheck && statusCheck;
        }

        /// <summary>
        /// Записывает в переданный поток ответы на запрос клиента
        /// </summary>
        private async Task WriteStreamAsync<TResponse>(IServerStreamWriter<TResponse> stream, TResponse reply) where TResponse : IMessage<TResponse>
        {
            try
            {
                await stream.WriteAsync(reply);
            }
            catch (Exception exception)
            {
                //TODO что делать когда разорвется соеденение ?
                Log.Logger.Warning("Connection was interrupted by network services.");
            }
        }

        public async override Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var trademarket = context.RequestHeaders.Get("trademarket").Value;

            var user = await _director.GetUserContextAsync(sessionId, slot, trademarket);

            /*user.UserBalance += async (sender, args) => {
                await WriteStreamAsync<SubscribeBalanceResponse>(responseStream, new SubscribeBalanceResponse { Response = new() { Balance = ConvertService.ConvertBalance(args.Changed) } });
            };*/
            //TODO отписка после отмены
            await AwaitCancellation(context.CancellationToken);

        }

        public async override Task SubscribePrice(SubscribePriceRequest request, IServerStreamWriter<SubscribePriceResponse> responseStream, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var trademarket = context.RequestHeaders.Get("trademarket").Value;

            var user = await _director.GetUserContextAsync(sessionId, slot, trademarket);

            /* user.InstrumentUpdate += async (sender, args) => {
                 await WriteStreamAsync<SubscribePriceResponse>(responseStream, ConvertService.ConvertInstrument(args.Changed,args.Action));
             };*/
            //TODO отписка после отмены
            await AwaitCancellation(context.CancellationToken);
        }

        public async override Task SubscribeMargin(SubscribeMarginRequest request, IServerStreamWriter<SubscribeMarginResponse> responseStream, ServerCallContext context)
        {
            Log.Logger.Information($"Connected with {context.Host}");

            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var trademarket = context.RequestHeaders.Get("trademarket").Value;


            var user = await _director.GetUserContextAsync(sessionId, slot, trademarket);
            /*user.UserMargin += async (sender, args) =>
            {
                var marginResponse = ConvertService.ConvertMargin(args.Changed, args.Action);
                //Log.Logger.Information($"Sent order : {order} to {context.Host}");
                await WriteStreamAsync<SubscribeMarginResponse>(responseStream, marginResponse);
            };*/
            //TODO отписка после отмены
            await AwaitCancellation(context.CancellationToken);

        }

        public async override Task SubscribePosition(SubscribePositionRequest request, IServerStreamWriter<SubscribePositionResponse> responseStream, ServerCallContext context)
        {
            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var trademarket = context.RequestHeaders.Get("trademarket").Value;

            var user = await _director.GetUserContextAsync(sessionId, slot, trademarket);


            /* user.UserPosition += async (sender, args) => {
                 await WriteStreamAsync<SubscribePositionResponse>(responseStream, ConvertService.ConvertPosition(args.Changed,args.Action));
             };*/
            //TODO отписка после отмены
            await AwaitCancellation(context.CancellationToken);
        }


        public async override Task SubscribeMyOrders(SubscribeMyOrdersRequest request, IServerStreamWriter<SubscribeMyOrdersResponse> responseStream, ServerCallContext context)
        {
            Log.Logger.Information($"Connected with {context.Host}");

            var sessionId = context.RequestHeaders.Get("sessionid").Value;
            var slot = context.RequestHeaders.Get("slot").Value;
            var trademarket = context.RequestHeaders.Get("trademarket").Value;
            var user = await _director.GetUserContextAsync(sessionId, slot, trademarket);
            /*user.UserOrders += async (sender, args) => {

                var response = ConvertService.ConvertMyOrder(args.Changed, args.Action);
                await WriteStreamAsync<SubscribeMyOrdersResponse>(responseStream, response);
            };*/
            //TODO отписка после отмены
            await AwaitCancellation(context.CancellationToken);
        }





       
       
    }
}
