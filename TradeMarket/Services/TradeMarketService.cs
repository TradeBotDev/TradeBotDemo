using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Instruments;
using Bitmex.Client.Websocket.Responses.Wallets;
using Google.Protobuf;
using Grpc.Core;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using TradeBot.TradeMarket.TradeMarketService.v1;
using TradeMarket.Model.Publishers;
using TradeMarket.Model.TradeMarkets;
using TradeMarket.Model.UserContexts;
using TradeMarket.Model.UserContexts.Builders;
using SubscribeBalanceRequest = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceRequest;
using SubscribeBalanceResponse = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceResponse;
using Margin = Bitmex.Client.Websocket.Responses.Margins.Margin;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Orders;
using System.Collections.Generic;
using Serilog.Context;
using Serilog.Enrichers;
using Serilog.Core.Enrichers;
using Bitmex.Client.Websocket.Responses;

namespace TradeMarket.Services
{
    public partial class TradeMarketService : TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceBase
    {
        private readonly ContextDirector _director;

        public TradeMarketService(ContextDirector director)
        {
            this._director = director;
        }

        #region Helpers
        //TODO Перевести все в одну функцию
        /// <summary>
        /// Переводит заголовки запроса в язык сервиса и предоставляет контекст пользователя по переданым заголовкам
        /// </summary>
        public async Task<UserContext> GetUserContextAsync(Metadata meta,ContextFilter.GetFilter getFilter,CancellationToken token)
        {
            return await Task.Run(async () =>
            {
                var sessionId = meta.Get("sessionid").Value;
                var slot = meta.Get("slot").Value;
                var trademarket = meta.Get("trademarket").Value;

                return await _director.GetUserContextAsync(getFilter(sessionId,slot,trademarket),token);
            });
        }


        /// <summary>
        /// Метод заполняет заголовки для ответов по предоставленному контексту пользователя 
        /// </summary>
        public async Task<Metadata> AddInfoToMetadataAsync(IContext user)
        {
            return await Task.Run(() =>
            {
                Metadata res = new Metadata();
                res.Add("sessionid", user.Signature.SessionId);
                res.Add("slot", user.Signature.SlotName);
                res.Add("trademarket", user.Signature.TradeMarketName);
                return res;
            });
        }

        /// <summary>
        /// Переносит данные о пользователе из запроса в ответ
        /// </summary>
        public async Task<Metadata> MoveInfoToMetadataAsync(Metadata requestMeta)
        {
            return await Task.Run(() =>
            {
                Metadata res = new Metadata();
                res.Add("sessionid",requestMeta.GetValue("sessionid"));
                res.Add("slot", requestMeta.GetValue("slot"));
                res.Add("trademarket", requestMeta.GetValue("trademarket"));
                return res;
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
        private async Task WriteStreamAsync<TResponse>(IServerStreamWriter<TResponse> stream, TResponse response) where TResponse : IMessage<TResponse>
        {
            try
            {
                Log.Information("Sent message {@message}",response);
                await stream.WriteAsync(response);
                Log.Information("Message sent succesful");
            }
            catch 
            {
                //TODO что делать когда разорвется соеденение ?
                Log.Logger.Error("Connection was interrupted by network services.");
                throw;
            }
        }

        public async Task SubscribeToUserTopic<TRequest, TResponse, TModel>(
            Func<EventHandler<IPublisher<TModel>.ChangedEventArgs>, CancellationToken, Task<List<TModel>>> subscribe,
            Func<EventHandler<IPublisher<TModel>.ChangedEventArgs>, Task> unsubscribe,
            TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            Func<TModel, bool> filter,
            Func<TModel, BitmexAction, TResponse> converter,
            ServerCallContext context)
            where TResponse : IMessage<TResponse>
            where TRequest : IMessage<TRequest>
        {
            void Handler(object sender, IPublisher<TModel>.ChangedEventArgs args)
            {
                //переводим из языка сервиса на язык протофайлов
                Log.Information("Sent position");
                var response = converter(args.Changed, args.Action);
                if (filter is null || filter(args.Changed))
                {
                    try
                    {
                        WriteStreamAsync(responseStream, response).Wait();
                    }
                    catch
                    {
                        Log.Error("Exception catched");
                        throw;
                    }
                }
            }
            await Task.Run(async () =>
            {
                try
                {
                    Log.Information("Canceletion requested : {@Token}", context.CancellationToken.IsCancellationRequested);

                    //Добавляем заголовки ответа по контексту пользователя user из запроса
                    var meta = await MoveInfoToMetadataAsync(context.RequestHeaders);
                    await context.WriteResponseHeadersAsync(meta);
                    Log.Information("Wrote Response Headers {@Meta}", context.ResponseTrailers);
                    //подписываемся на обновления
                    var cache = await subscribe(Handler, context.CancellationToken);
                    Log.Information("Cache Contains {@CacheCount} elements", cache.Count);


                    //кидаем данные из кэша
                    Parallel.ForEach(cache, data => Handler(this, new(data, Bitmex.Client.Websocket.Responses.BitmexAction.Partial)));

                    //ожидаем пока клиенты отменят подписку
                    await AwaitCancellation(context.CancellationToken);
                    Log.Information("Connection was Canceled");
                    context.Status = Status.DefaultSuccess;
                }
                catch (Exception e)
                {
                    //записываем ошибку в логер
                    Log.Error(e.Message);
                    Log.Error(e.StackTrace);
                    //ставим статус "Отмена" в заголовке ответа
                    context.Status = Status.DefaultCancelled;
                }
                finally
                {
                    //отписываемся от обновлений по книге
                    Log.Information("Finnaly unsubscribed before");
                    await unsubscribe(Handler);
                    Log.Information("Finnaly unsubscribed after");

                }
            }
            );
        }
        #endregion

        #region Order Commands

        /// <summary>
        /// Размещает ордер на выбранной бирже
        /// </summary>
        public async override Task<TradeBot.TradeMarket.TradeMarketService.v1.PlaceOrderResponse> PlaceOrder(TradeBot.TradeMarket.TradeMarketService.v1.PlaceOrderRequest request, ServerCallContext context)
        {
            using (LogContext.Push(new PropertyEnricher("RPC Method", context.Method),
             new PropertyEnricher("RequestId", Guid.NewGuid().ToString()),
             new PropertyEnricher("UserSessionId", context.RequestHeaders.Get("sessionid").Value),
             new PropertyEnricher("Command", "Place")))
            {
                UserContext user = null;
                try
                {
                    //ищем конеткст пользователя
                    user = await GetUserContextAsync(context.RequestHeaders, ContextFilter.GetFullContextFilter, context.CancellationToken);
                    //отправляем запрос на биржу. TODO как работает тут токен отмены
                    var response = await user.PlaceOrder(request.Value, request.Price, context.CancellationToken);

                    //ставим статус запроса как успешный
                    context.Status = Status.DefaultSuccess;
                    //конвертируем из внутреннего типа сервиса в тип grpc
                    return ConvertService.ConvertPlaceOrderResponse(response);

                }
                catch (Exception e)
                {
                    //записываем ошибку в логер
                    Log.Logger.Error(e.Message);
                    //ставим статус "Отменен" в заголовке ответа
                    context.Status = Status.DefaultCancelled;
                }
                //TODO вообще не красиво
                return new()
                {
                    OrderId = "",
                    Response = new()
                    {
                        Code = TradeBot.Common.v1.ReplyCode.Failure,
                        Message = "Inner Error"
                    }
                };

            } 
        }

        /// <summary>
        /// Изменяет уже выставленный оредр на выбранной бирже в выбранном слоте
        /// </summary>
        public async override Task<TradeBot.TradeMarket.TradeMarketService.v1.AmmendOrderResponse> AmmendOrder(TradeBot.TradeMarket.TradeMarketService.v1.AmmendOrderRequest request, ServerCallContext context)
        {
            using (LogContext.Push(new PropertyEnricher("RPC Method", context.Method),
             new PropertyEnricher("RequestId", Guid.NewGuid().ToString()),
             new PropertyEnricher("UserSessionId", context.RequestHeaders.Get("sessionid").Value),
             new PropertyEnricher("Command", "Ammend")))
            {
                try
                {
                    //находим пользователя 
                    var user = await GetUserContextAsync(context.RequestHeaders, ContextFilter.GetFullContextFilter, context.CancellationToken);

                    // переводим параметры grpc запроса на язык сервиса
                    double? price = 0;
                    switch (request.PriceType)
                    {
                        case PriceType.Default: price = request.NewPrice; break;
                        case PriceType.None: price = null; break;
                        case PriceType.Unspecified: throw new RpcException(Status.DefaultCancelled, $"{nameof(request.PriceType)} should be specified");
                    }
                    long? quantity = null, leavesQuantity = null;
                    switch (request.QuantityType)
                    {
                        case QuantityType.Leaves: leavesQuantity = request.NewQuantity; break;
                        case QuantityType.Default: quantity = request.NewQuantity; break;
                        case QuantityType.None: break;
                        case QuantityType.Unspecified: throw new RpcException(Status.DefaultCancelled, $"{nameof(request.QuantityType)} should be specified");
                    }
                    //отправляем запрос на биржу
                    var response = await user.AmmendOrder(request.Id, price, quantity, leavesQuantity, context.CancellationToken);
                    //ставим статус запроса как успешный
                    context.Status = Status.DefaultSuccess;
                    //конвертируем из внутреннего типа сервиса в тип grpc
                    return ConvertService.ConvertAmmendOrderResponse(response);
                }
                catch (Exception e)
                {
                    //записываем ошибку в логер
                    Log.Logger.Error(e.Message);
                    //ставим статус "Отменен" в заголовке ответа
                    context.Status = Status.DefaultCancelled;
                }
                //TODO вообще не красиво
                return new()
                {
                    Response = new()
                    {
                        Code = TradeBot.Common.v1.ReplyCode.Failure,
                        Message = "Inner Error"
                    }
                };
            }
        }

        /// <summary>
        /// Удаляет уже выставленный ордер на выбранной бирже в выбраном слоте
        /// </summary>
        public async override Task<DeleteOrderResponse> DeleteOrder(DeleteOrderRequest request, ServerCallContext context)
        {
            using (LogContext.Push(new PropertyEnricher("RPC Method", context.Method),
             new PropertyEnricher("RequestId", Guid.NewGuid().ToString()),
             new PropertyEnricher("UserSessionId", context.RequestHeaders.Get("sessionid").Value),
             new PropertyEnricher("Command", "Delete")))
            {
                UserContext user = null;
                try
                {
                    //ищем конеткст пользователя
                    user = await GetUserContextAsync(context.RequestHeaders, ContextFilter.GetFullContextFilter, context.CancellationToken);

                    //отправляем запрос на биржу. TODO как работает тут токен отмены
                    var response = await user.DeleteOrder(request.OrderId, context.CancellationToken);

                    //ставим статус запроса как успешный
                    context.Status = Status.DefaultSuccess;
                    //конвертируем из внутреннего типа сервиса в тип grpc
                    return ConvertService.ConvertDeleteOrderResponse(response);

                }
                catch (Exception e)
                {
                    //записываем ошибку в логер
                    Log.Logger.Error(e.Message);
                    //ставим статус "Отменен" в заголовке ответа
                    context.Status = Status.DefaultCancelled;
                }
                //TODO вообще не красиво
                return new()
                {
                    Response = new()
                    {
                        Code = TradeBot.Common.v1.ReplyCode.Failure,
                        Message = "Inner Error"
                    }
                };
            }
        }
        #endregion

        #region Subscribe Commands

        public async override Task SubscribePrice(SubscribePriceRequest request, IServerStreamWriter<SubscribePriceResponse> responseStream, ServerCallContext context)
        {
           
            using (LogContext.Push(new PropertyEnricher("RPC Method", context.Method), new PropertyEnricher("RequestId", Guid.NewGuid().ToString()), new PropertyEnricher("UserSessionId", context.RequestHeaders.Get("sessionid"))))
            {
                Log.Information("Starting subscriprion");

                var common = await GetUserContextAsync(context.RequestHeaders, ContextFilter.GetCommonContextFilter, context.CancellationToken);
                await SubscribeToUserTopic<SubscribePriceRequest, SubscribePriceResponse, Instrument>(
                    common.SubscribeToInstrumentUpdate, 
                    common.UnSubscribeFromInstrumentUpdate, 
                    request, 
                    responseStream, 
                    (x) => x.Symbol == context.RequestHeaders.GetValue("slot"),
                    ConvertService.ConvertInstrument,
                    context);
            }
        }

        public async override Task SubscribeMargin(SubscribeMarginRequest request, IServerStreamWriter<SubscribeMarginResponse> responseStream, ServerCallContext context)
        {
            
            using (LogContext.Push(new PropertyEnricher("RPC Method", context.Method),
              new PropertyEnricher("RequestId", Guid.NewGuid().ToString()),
              new PropertyEnricher("UserSessionId", context.RequestHeaders.GetValue("sessionid")),
              new PropertyEnricher("Topic", "Margin")))
            {
                Log.Information("Starting subscriprion for {@Topic}", "margin");

                var user = await GetUserContextAsync(context.RequestHeaders,ContextFilter.GetTradeMarketContextFilter, context.CancellationToken);
                await SubscribeToUserTopic<SubscribeMarginRequest, SubscribeMarginResponse, Margin>(
                    user.SubscribeToUserMargin, 
                    user.UnSubscribeFromUserMargin,
                    request, 
                    responseStream, 
                    null,
                    ConvertService.ConvertMargin,
                    context);
            }

        }

        public async override Task SubscribePosition(SubscribePositionRequest request, IServerStreamWriter<SubscribePositionResponse> responseStream, ServerCallContext context)
        {
            using (LogContext.Push(new PropertyEnricher("RPC Method", context.Method),
              new PropertyEnricher("RequestId", Guid.NewGuid().ToString()),
              new PropertyEnricher("UserSessionId", context.RequestHeaders.GetValue("sessionid")),
              new PropertyEnricher("Topic", "Position")))
            {
            var user = await GetUserContextAsync(context.RequestHeaders,ContextFilter.GetTradeMarketContextFilter, context.CancellationToken);
            await SubscribeToUserTopic<SubscribePositionRequest, SubscribePositionResponse, Position>(
                user.SubscribeToUserPositions, 
                user.UnSubscribeFromUserPositions,
                request, 
                responseStream, 
                (x) => x.Symbol == context.RequestHeaders.GetValue("slot"),
                ConvertService.ConvertPosition,
                context);
            }
        }


        public async override Task SubscribeMyOrders(SubscribeMyOrdersRequest request, IServerStreamWriter<SubscribeMyOrdersResponse> responseStream, ServerCallContext context)
        {
            using (LogContext.Push(new PropertyEnricher("RPC Method", context.Method),
              new PropertyEnricher("RequestId", Guid.NewGuid().ToString()),
              new PropertyEnricher("UserSessionId", context.RequestHeaders.Get("sessionid").Value),
              new PropertyEnricher("Topic", "Orders")))
            {
                Log.Information("Starting subscriprion for {@Topic}", "user orders");

                var user = await GetUserContextAsync(context.RequestHeaders,ContextFilter.GetTradeMarketContextFilter,context.CancellationToken);
                await SubscribeToUserTopic<SubscribeMyOrdersRequest, SubscribeMyOrdersResponse, Order>(
                    user.SubscribeToUserOrders, 
                    user.UnSubscribeFromUserOrders,
                    request, 
                    responseStream,
                    null,
                    ConvertService.ConvertMyOrder,
                    context);
            }

            
        }


        /// <summary>
        /// Метод дает доступ к обновлениям стаканов выбранной биржы выбранного слота
        /// </summary>
        public override async Task SubscribeOrders(TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersRequest request, IServerStreamWriter<SubscribeOrdersResponse> responseStream, ServerCallContext context)
        {
            using (LogContext.Push(new PropertyEnricher("RPC Method", context.Method),
              new PropertyEnricher("RequestId", Guid.NewGuid().ToString()),
              new PropertyEnricher("UserSessionId", context.RequestHeaders.Get("sessionid").Value),
              new PropertyEnricher("Topic", "BookLevel")))
            {
            Log.Information("Starting subscriprion for {@Topic}", "booklevel25");

            var common = await GetUserContextAsync(context.RequestHeaders,ContextFilter.GetCommonContextFilter,context.CancellationToken);
            await SubscribeToUserTopic<SubscribeOrdersRequest, SubscribeOrdersResponse, BookLevel>(
                common.SubscribeToBook25UpdatesAsync, 
                common.UnSubscribeFromBook25UpdatesAsync,
                request, 
                responseStream,
                null,
                ConvertService.ConvertBookOrders,
                context);
            }
        }

        /// <summary>
        /// Доступ к ежедневному обновлению баланса пользователя
        /// </summary>
        public async override Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {
            using (LogContext.Push(new PropertyEnricher("RPC Method", context.Method),
             new PropertyEnricher("RequestId", Guid.NewGuid().ToString()),
             new PropertyEnricher("UserSessionId", context.RequestHeaders.Get("sessionid").Value),
             new PropertyEnricher("Topic", "Wallet")))
            {
            Log.Information("Starting subscriprion for {@Topic}", "balance");
            //находим общий контекст т.к. подписка на стаканы не требует логина в систему биржи
            var user = await GetUserContextAsync(context.RequestHeaders,ContextFilter.GetTradeMarketContextFilter ,context.CancellationToken);
            await SubscribeToUserTopic<SubscribeBalanceRequest,SubscribeBalanceResponse,Wallet>(
                user.SubscribeToBalance, 
                user.UnSubscribeFromBalance,
                request,
                responseStream, 
                null,
                ConvertService.ConvertBalance,
                context);
            }
        }

        #endregion

        



    }
}
