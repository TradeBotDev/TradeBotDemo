using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Instruments;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Wallets;
using Google.Protobuf;
using Grpc.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeBot.TradeMarket.TradeMarketService.v1;
using TradeMarket.Model.Publishers;
using TradeMarket.Model.UserContexts;
using TradeMarket.Model.UserContexts.Builders;
using Margin = Bitmex.Client.Websocket.Responses.Margins.Margin;
using SubscribeBalanceRequest = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceRequest;
using SubscribeBalanceResponse = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceResponse;

namespace TradeMarket.Services
{
    public partial class TradeMarketService : TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceBase
    {
        private readonly ContextDirector _director;

        private ILogger _logger;

        public TradeMarketService(ContextDirector director)
        {
            this._director = director;
            _logger = Log.ForContext("Where", "TradeMarketService");
        }

        #region Helpers
        //TODO Перевести все в одну функцию
        /// <summary>
        /// Переводит заголовки запроса в язык сервиса и предоставляет контекст пользователя по переданым заголовкам
        /// </summary>
        public async Task<UserContext> GetUserContextAsync(Metadata meta, ContextFilter.GetFilter getFilter, CancellationToken token, ILogger logger)
        {
            return await Task.Run(async () =>
            {
                var sessionId = meta.GetValue("sessionid");
                var slot = meta.GetValue("slot");
                var trademarket = meta.GetValue("trademarket");

                return await _director.GetUserContextAsync(getFilter(sessionId, slot, trademarket), token, logger);
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
                res.Add("sessionid", requestMeta.GetValue("sessionid"));
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
        private async Task WriteStreamAsync<TResponse>(IServerStreamWriter<TResponse> stream, TResponse response,ILogger logger) where TResponse : IMessage<TResponse>
        {
            try
            {
                logger.Information("Sent message {@message}", response);
                await stream.WriteAsync(response);
                logger.Information("Message sent succesful");
            }
            catch
            {
                //TODO что делать когда разорвется соеденение ?
                logger.Error("Connection was interrupted by network services.");
                throw;
            }
        }

        public async Task SubscribeToUserTopic<TRequest, TResponse, TModel>(
            Func<EventHandler<IPublisher<TModel>.ChangedEventArgs>, CancellationToken, ILogger, Task<List<TModel>>> subscribe,
            Func<EventHandler<IPublisher<TModel>.ChangedEventArgs>, ILogger, Task> unsubscribe,
            TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            Func<TModel, bool> filter,
            Func<TModel, BitmexAction, TResponse> converter,
            ServerCallContext context,
            ILogger logger)
            where TResponse : IMessage<TResponse>
            where TRequest : IMessage<TRequest>
        {
            var log = logger.ForContext("Method", nameof(SubscribeToUserTopic));
            void Handler(object sender, IPublisher<TModel>.ChangedEventArgs args)
            {
                //переводим из языка сервиса на язык протофайлов
                log.Information("Sent position");
                var response = converter(args.Changed, args.Action);
                if (filter is null || filter(args.Changed))
                {
                    try
                    {
                        WriteStreamAsync(responseStream, response,log).Wait();
                    }
                    catch
                    {
                        log.Error("Exception catched");
                        throw;
                    }
                }
            }
            await Task.Run(async () =>
            {
                try
                {
                    log.Information("Canceletion requested : {@Token}", context.CancellationToken.IsCancellationRequested);

                    //Добавляем заголовки ответа по контексту пользователя user из запроса
                    var meta = await MoveInfoToMetadataAsync(context.RequestHeaders);
                    await context.WriteResponseHeadersAsync(meta);
                    log.Information("Wrote Response Headers {@Meta}", context.ResponseTrailers);
                    //подписываемся на обновления
                    var cache = await subscribe(Handler, context.CancellationToken, log);
                    log.Information("Cache Contains {@CacheCount} elements", cache.Count);


                    //кидаем данные из кэша
                    Parallel.ForEach(cache, data => Handler(this, new(data, Bitmex.Client.Websocket.Responses.BitmexAction.Partial)));

                    //ожидаем пока клиенты отменят подписку
                    await AwaitCancellation(context.CancellationToken);
                    log.Information("Connection was Canceled");
                    context.Status = Status.DefaultSuccess;
                }
                catch (Exception e)
                {
                    //записываем ошибку в логер
                    log.Error(e.Message);
                    log.Error(e.StackTrace);
                    //ставим статус "Отмена" в заголовке ответа
                    context.Status = Status.DefaultCancelled;
                }
                finally
                {
                    //отписываемся от обновлений по книге
                    log.Information("Finnaly unsubscribed before");
                    await unsubscribe(Handler, log);
                    log.Information("Finnaly unsubscribed after");

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
            var logger = _logger.
                   ForContext<TradeMarketService>().
                   ForContext("RPC Method", context.Method).
                   ForContext("RequestId", Guid.NewGuid().ToString()).
                   ForContext("UserSessionId", context.RequestHeaders.GetValue("sessionid")).
                   ForContext("Command", "Place");

            UserContext user = null;
            try
            {
                logger.Information("Request : {@Request}", request);
                //ищем конеткст пользователя
                user = await GetUserContextAsync(context.RequestHeaders, ContextFilter.GetFullContextFilter, context.CancellationToken, logger);
                //отправляем запрос на биржу. TODO как работает тут токен отмены
                var response = await user.PlaceOrder(request.Value, request.Price, context.CancellationToken, logger);

                //ставим статус запроса как успешный
                context.Status = Status.DefaultSuccess;
                //конвертируем из внутреннего типа сервиса в тип grpc
                return ConvertService.ConvertPlaceOrderResponse(response);

            }
            catch (Exception e)
            {
                //записываем ошибку в логер
                logger.Error(e.Message);
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

        /// <summary>
        /// Изменяет уже выставленный оредр на выбранной бирже в выбранном слоте
        /// </summary>
        public async override Task<TradeBot.TradeMarket.TradeMarketService.v1.AmmendOrderResponse> AmmendOrder(TradeBot.TradeMarket.TradeMarketService.v1.AmmendOrderRequest request, ServerCallContext context)
        {
            var logger = _logger.
                   ForContext<TradeMarketService>().
                   ForContext("RPC Method", context.Method).
                   ForContext("RequestId", Guid.NewGuid().ToString()).
                   ForContext("UserSessionId", context.RequestHeaders.GetValue("sessionid")).
                   ForContext("Command", "Ammend");

            try
            {
                logger.Information("Request : {@Request}", request);

                //находим пользователя 
                var user = await GetUserContextAsync(context.RequestHeaders, ContextFilter.GetFullContextFilter, context.CancellationToken, logger);

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
                var response = await user.AmmendOrder(request.Id, price, quantity, leavesQuantity, context.CancellationToken, logger);
                //ставим статус запроса как успешный
                context.Status = Status.DefaultSuccess;
                //конвертируем из внутреннего типа сервиса в тип grpc
                return ConvertService.ConvertAmmendOrderResponse(response);
            }
            catch (Exception e)
            {
                //записываем ошибку в логер
                logger.Error(e.Message);
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

        /// <summary>
        /// Удаляет уже выставленный ордер на выбранной бирже в выбраном слоте
        /// </summary>
        public async override Task<DeleteOrderResponse> DeleteOrder(DeleteOrderRequest request, ServerCallContext context)
        {
            var logger = _logger.
                   ForContext<TradeMarketService>().
                   ForContext("RPC Method", context.Method).
                   ForContext("RequestId", Guid.NewGuid().ToString()).
                   ForContext("UserSessionId", context.RequestHeaders.GetValue("sessionid")).
                   ForContext("Command", "Delete");
            UserContext user = null;
            try
            {
                logger.Information("Request : {@Request}", request);

                //ищем конеткст пользователя
                user = await GetUserContextAsync(context.RequestHeaders, ContextFilter.GetFullContextFilter, context.CancellationToken, logger);

                //отправляем запрос на биржу. TODO как работает тут токен отмены
                var response = await user.DeleteOrder(request.OrderId, context.CancellationToken, logger);

                //ставим статус запроса как успешный
                context.Status = Status.DefaultSuccess;
                //конвертируем из внутреннего типа сервиса в тип grpc
                return ConvertService.ConvertDeleteOrderResponse(response);

            }
            catch (Exception e)
            {
                //записываем ошибку в логер
                logger.Error(e.Message);
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
        #endregion

        #region Subscribe Commands

        public async override Task SubscribePrice(SubscribePriceRequest request, IServerStreamWriter<SubscribePriceResponse> responseStream, ServerCallContext context)
        {

            var logger = _logger.
                ForContext<TradeMarketService>().
                ForContext("RPC Method", context.Method).
                ForContext("RequestId", Guid.NewGuid().ToString()).
                ForContext("UserSessionId", context.RequestHeaders.GetValue("sessionid")).
                ForContext("Topic", "Instrument");

            logger.Information("Request : {@Request}", request);


            var common = await GetUserContextAsync(context.RequestHeaders, ContextFilter.GetCommonContextFilter, context.CancellationToken, logger);
            await SubscribeToUserTopic<SubscribePriceRequest, SubscribePriceResponse, Instrument>(
                common.SubscribeToInstrumentUpdate,
                common.UnSubscribeFromInstrumentUpdate,
                request,
                responseStream,
                (x) => x.Symbol == context.RequestHeaders.GetValue("slot"),
                ConvertService.ConvertInstrument,
                context,
                logger);

        }

        public async override Task SubscribeMargin(SubscribeMarginRequest request, IServerStreamWriter<SubscribeMarginResponse> responseStream, ServerCallContext context)
        {

            var logger = _logger.
                ForContext<TradeMarketService>().
                ForContext("RPC Method", context.Method).
                ForContext("RequestId", Guid.NewGuid().ToString()).
                ForContext("UserSessionId", context.RequestHeaders.GetValue("sessionid")).
                ForContext("Topic", "Margin");
            logger.Information("Request : {@Request}", request);



            var user = await GetUserContextAsync(context.RequestHeaders, ContextFilter.GetTradeMarketContextFilter, context.CancellationToken, logger);
            await SubscribeToUserTopic<SubscribeMarginRequest, SubscribeMarginResponse, Margin>(
                user.SubscribeToUserMargin,
                user.UnSubscribeFromUserMargin,
                request,
                responseStream,
                null,
                ConvertService.ConvertMargin,
                context,
                logger);


        }

        public async override Task SubscribePosition(SubscribePositionRequest request, IServerStreamWriter<SubscribePositionResponse> responseStream, ServerCallContext context)
        {
            var logger = _logger.
               ForContext<TradeMarketService>().
               ForContext("RPC Method", context.Method).
               ForContext("RequestId", Guid.NewGuid().ToString()).
               ForContext("UserSessionId", context.RequestHeaders.GetValue("sessionid")).
               ForContext("Topic", "Position");
            logger.Information("Request : {@Request}", request);

            var user = await GetUserContextAsync(context.RequestHeaders, ContextFilter.GetTradeMarketContextFilter, context.CancellationToken, logger);
            await SubscribeToUserTopic<SubscribePositionRequest, SubscribePositionResponse, Position>(
                user.SubscribeToUserPositions,
                user.UnSubscribeFromUserPositions,
                request,
                responseStream,
                (x) => x.Symbol == context.RequestHeaders.GetValue("slot"),
                ConvertService.ConvertPosition,
                context,
                logger);

        }


        public async override Task SubscribeMyOrders(SubscribeMyOrdersRequest request, IServerStreamWriter<SubscribeMyOrdersResponse> responseStream, ServerCallContext context)
        {
            var logger = _logger.
               ForContext<TradeMarketService>().
               ForContext("RPC Method", context.Method).
               ForContext("RequestId", Guid.NewGuid().ToString()).
               ForContext("UserSessionId", context.RequestHeaders.GetValue("sessionid")).
               ForContext("Topic", "Orders");
            logger.Information("Request : {@Request}", request);

            var user = await GetUserContextAsync(context.RequestHeaders, ContextFilter.GetTradeMarketContextFilter, context.CancellationToken, logger);
            await SubscribeToUserTopic<SubscribeMyOrdersRequest, SubscribeMyOrdersResponse, Order>(
                user.SubscribeToUserOrders,
                user.UnSubscribeFromUserOrders,
                request,
                responseStream,
                null,
                ConvertService.ConvertMyOrder,
                context,
                logger);



        }


        /// <summary>
        /// Метод дает доступ к обновлениям стаканов выбранной биржы выбранного слота
        /// </summary>
        public override async Task SubscribeOrders(TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersRequest request, IServerStreamWriter<SubscribeOrdersResponse> responseStream, ServerCallContext context)
        {
            var logger = _logger.
                   ForContext<TradeMarketService>().
                   ForContext("RPC Method", context.Method).
                   ForContext("RequestId", Guid.NewGuid().ToString()).
                   ForContext("UserSessionId", context.RequestHeaders.GetValue("sessionid")).
                   ForContext("Topic", "BookLevel25");
            logger.Information("Request : {@Request}", request);

            var common = await GetUserContextAsync(context.RequestHeaders, ContextFilter.GetCommonContextFilter, context.CancellationToken, logger);
            await SubscribeToUserTopic<SubscribeOrdersRequest, SubscribeOrdersResponse, BookLevel>(
                common.SubscribeToBook25UpdatesAsync,
                common.UnSubscribeFromBook25UpdatesAsync,
                request,
                responseStream,
                null,
                ConvertService.ConvertBookOrders,
                context,
                logger);

        }

        /// <summary>
        /// Доступ к ежедневному обновлению баланса пользователя
        /// </summary>
        public async override Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {
            var logger = _logger.
                  ForContext<TradeMarketService>().
                  ForContext("RPC Method", context.Method).
                  ForContext("RequestId", Guid.NewGuid().ToString()).
                  ForContext("UserSessionId", context.RequestHeaders.GetValue("sessionid")).
                  ForContext("Topic", "Wallet");

            logger.Information("Request : {@Request}", request);

            //находим общий контекст т.к. подписка на стаканы не требует логина в систему биржи
            var user = await GetUserContextAsync(context.RequestHeaders, ContextFilter.GetTradeMarketContextFilter, context.CancellationToken, logger);
            await SubscribeToUserTopic<SubscribeBalanceRequest, SubscribeBalanceResponse, Wallet>(
                user.SubscribeToBalance,
                user.UnSubscribeFromBalance,
                request,
                responseStream,
                null,
                ConvertService.ConvertBalance,
                context,
                logger);

        }

        #endregion





    }
}
