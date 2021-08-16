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

        #region Helpers
        /// <summary>
        /// Переводит заголовки запроса в язык сервиса и предоставляет контекст пользователя по переданым заголовкам
        /// </summary>
        public async Task<UserContext> GetUserContextAsync(Metadata meta,CancellationToken token)
        {
            return await Task.Run(async () =>
            {
                var sessionId = meta.Get("sessionid").Value;
                var slot = meta.Get("slot").Value;
                var trademarket = meta.Get("trademarket").Value;

                return await _director.GetUserContextAsync(sessionId, slot, trademarket,token);
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
        /// Переносит данные о пользователе из запроса в ответ
        /// </summary>
        public async Task<Metadata> MoveInfoToMetadataAsync(Metadata requestMeta,Metadata responseMeta)
        {
            return await Task.Run(() =>
            {
                responseMeta.Add("sessionid",requestMeta.GetValue("sessionid"));
                responseMeta.Add("slot", requestMeta.GetValue("slot"));
                responseMeta.Add("trademarket", requestMeta.GetValue("trademarket"));
                return responseMeta;
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

        public async Task SubscribeToUserTopic<TRequest, TResponse, TModel>(Func<EventHandler<IPublisher<TModel>.ChangedEventArgs>, CancellationToken, Task> subscribe, Func<EventHandler<IPublisher<TModel>.ChangedEventArgs>, Task> unsubscribe, EventHandler<IPublisher<TModel>.ChangedEventArgs> handler, TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context)
        {
            try
            {
                //Добавляем заголовки ответа по контексту пользователя user из запроса
                await MoveInfoToMetadataAsync(context.RequestHeaders, context.ResponseTrailers);

                await subscribe(handler, /*context.CancellationToken*/new CancellationToken());
                //ожидаем пока клиенты отменят подписку
                await AwaitCancellation(/*context.CancellationToken*/ new CancellationToken());
            }
            catch (Exception e)
            {
                //записываем ошибку в логер
                Log.Logger.Error(e.Message);
                Log.Logger.Error(e.StackTrace);
                //ставим статус "Отмена" в заголовке ответа
                context.Status = Status.DefaultCancelled;
            }
            finally
            {
                //отписываемся от обновлений по книге
                await unsubscribe(handler);
            }
        }

        #endregion

        #region Order Commands

        /// <summary>
        /// Размещает ордер на выбранной бирже
        /// </summary>
        public async override Task<TradeBot.TradeMarket.TradeMarketService.v1.PlaceOrderResponse> PlaceOrder(TradeBot.TradeMarket.TradeMarketService.v1.PlaceOrderRequest request, ServerCallContext context)
        {
            UserContext user = null;
            try
            {
                //ищем конеткст пользователя
                user = await GetUserContextAsync(context.RequestHeaders, context.CancellationToken);
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

        /// <summary>
        /// Изменяет уже выставленный оредр на выбранной бирже в выбранном слоте
        /// </summary>
        public async override Task<TradeBot.TradeMarket.TradeMarketService.v1.AmmendOrderResponse> AmmendOrder(TradeBot.TradeMarket.TradeMarketService.v1.AmmendOrderRequest request, ServerCallContext context)
        {
            try
            {
                //находим пользователя 
                var user = await GetUserContextAsync(context.RequestHeaders, context.CancellationToken);

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

        /// <summary>
        /// Удаляет уже выставленный ордер на выбранной бирже в выбраном слоте
        /// </summary>
        public async override Task<DeleteOrderResponse> DeleteOrder(DeleteOrderRequest request, ServerCallContext context)
        {
            UserContext user = null;
            try
            {
                //ищем конеткст пользователя
                user = await GetUserContextAsync(context.RequestHeaders, context.CancellationToken);

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
        #endregion

        #region Subscribe Commands


        /// <summary>
        /// Метод дает доступ к обновлениям стаканов выбранной биржы выбранного слота
        /// </summary>
        public override async Task SubscribeOrders(TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersRequest request, IServerStreamWriter<SubscribeOrdersResponse> responseStream, ServerCallContext context)
        {

            //тут void потому что ивент по другому не позволяет 
            async void WriteToStreamAsync(object sender, Model.Publishers.IPublisher<Bitmex.Client.Websocket.Responses.Books.BookLevel>.ChangedEventArgs args)
            {
                //переводим из языка сервиса на язык протофайлов
                var response = ConvertService.ConvertBookOrders(args.Changed, args.Action);
                //Проверяем подходит ли ордер из бирже по сигнатуре запроса {<продажа,покупка> , <открыт,закрыт>}
                if (IsOrderSuitForSignature(response.Response.Order.Signature, request.Request.Signature))
                {
                    //если ордер подходит то записываем его в поток ответов
                    await WriteStreamAsync(responseStream, response);
                }
            }

            var common = await GetCommonContextAsync(context.RequestHeaders);
            SubscribeToUserTopic<SubscribeOrdersRequest, SubscribeOrdersResponse, BookLevel>(common.SubscribeToBook25UpdatesAsync, common.UnSubscribeFromBook25UpdatesAsync, WriteToStreamAsync, request, responseStream, context);
        }

        /// <summary>
        /// Доступ к ежедневному обновлению баланса пользователя
        /// </summary>
        public async override Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<SubscribeBalanceResponse> responseStream, ServerCallContext context)
        {

            //тут void потому что ивент по другому не позволяет 
            async void WriteToStreamAsync(object sender, IPublisher<Wallet>.ChangedEventArgs args)
            {
                //переводим из языка сервиса на язык протофайлов
                var response = ConvertService.ConvertBalance(args.Changed, args.Action);

                await WriteStreamAsync(responseStream, response);

            }

            //находим общий контекст т.к. подписка на стаканы не требует логина в систему биржи
            var user = await GetUserContextAsync(context.RequestHeaders, context.CancellationToken);
            await SubscribeToUserTopic<SubscribeBalanceRequest,SubscribeBalanceResponse,Wallet>(user.SubscribeToBalance, user.UnSubscribeFromBalance, WriteToStreamAsync, request, responseStream, context);
        }

        #endregion

        public async override Task SubscribePrice(SubscribePriceRequest request, IServerStreamWriter<SubscribePriceResponse> responseStream, ServerCallContext context)
        {
            async void WriteToStreamAsync(object sender, IPublisher<Instrument>.ChangedEventArgs args)
            {
                //переводим из языка сервиса на язык протофайлов
                var response = ConvertService.ConvertInstrument(args.Changed, args.Action);
                await WriteStreamAsync(responseStream, response);

            }
            var common = await GetCommonContextAsync(context.RequestHeaders);
            await SubscribeToUserTopic<SubscribePriceRequest, SubscribePriceResponse, Instrument>(common.SubscribeToInstrumentUpdate, common.UnSubscribeFromInstrumentUpdate, WriteToStreamAsync, request, responseStream,context);
        }

        public async override Task SubscribeMargin(SubscribeMarginRequest request, IServerStreamWriter<SubscribeMarginResponse> responseStream, ServerCallContext context)
        {
            async void WriteToStreamAsync(object sender, IPublisher<Margin>.ChangedEventArgs args)
            {
                //переводим из языка сервиса на язык протофайлов
                var response = ConvertService.ConvertMargin(args.Changed, args.Action);
                await WriteStreamAsync(responseStream, response);

            }
            var user = await GetUserContextAsync(context.RequestHeaders, context.CancellationToken);
            await SubscribeToUserTopic<SubscribeMarginRequest, SubscribeMarginResponse, Margin>(user.SubscribeToUserMargin, user.UnSubscribeFromUserMargin, WriteToStreamAsync, request, responseStream, context);


        }

        public async override Task SubscribePosition(SubscribePositionRequest request, IServerStreamWriter<SubscribePositionResponse> responseStream, ServerCallContext context)
        {
            async void WriteToStreamAsync(object sender, IPublisher<Position>.ChangedEventArgs args)
            {
                //переводим из языка сервиса на язык протофайлов
                var response = ConvertService.ConvertPosition(args.Changed, args.Action);
                await WriteStreamAsync(responseStream, response);

            }
            var user = await GetUserContextAsync(context.RequestHeaders, context.CancellationToken);
            await SubscribeToUserTopic<SubscribePositionRequest,SubscribePositionResponse,Position>(user.SubscribeToUserPositions,user.UnSubscribeFromUserPositions,WriteToStreamAsync,request,responseStream,context);
        }


        public async override Task SubscribeMyOrders(SubscribeMyOrdersRequest request, IServerStreamWriter<SubscribeMyOrdersResponse> responseStream, ServerCallContext context)
        {
            async void WriteToStreamAsync(object sender, IPublisher<Order>.ChangedEventArgs args)
            {
                //переводим из языка сервиса на язык протофайлов
                var response = ConvertService.ConvertMyOrder(args.Changed, args.Action);
                await WriteStreamAsync(responseStream, response);

            }
            var user = await GetUserContextAsync(context.RequestHeaders, context.CancellationToken);
            await SubscribeToUserTopic<SubscribeMyOrdersRequest,SubscribeMyOrdersResponse, Order>(user.SubscribeToUserOrders, user.UnSubscribeFromUserOrders, WriteToStreamAsync, request, responseStream, context);

        }



    }
}
