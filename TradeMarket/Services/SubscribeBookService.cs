using Grpc.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.TradeMarket.TradeMarketService.v1;
using TradeMarket.Model.UserContexts;

namespace TradeMarket.Services
{
    /// <summary>
    /// Подписка на стаканы биржи
    /// </summary>
    public partial class TradeMarketService : TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceBase
    {
        /// <summary>
        /// Метод дает доступ к обновлениям стаканов выбранной биржы выбранного слота
        /// </summary>
        public async override Task SubscribeOrders(TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersRequest request, IServerStreamWriter<SubscribeOrdersResponse> responseStream, ServerCallContext context)
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

            CommonContext commonContext = null;
            UserContext user = null;
            try
            {
                //находим общий контекст т.к. подписка на стаканы не требует логина в систему биржи
                commonContext = await GetCommonContextAsync(context.RequestHeaders);
                //находим контекст пользователя для заполенния заголовков ответа чтобы сервисы понимали для кого они скачивают данные
                user = await GetUserContextAsync(context.RequestHeaders);
                //Добавляем заголовки ответа по контексту пользователя user 
                await AddInfoToMetadataAsync(user, context.ResponseTrailers);

                await commonContext.SubscribeToBook25UpdatesAsync(WriteToStreamAsync, context.CancellationToken);
                //ожидаем пока клиенты отменят подписку
                await AwaitCancellation(context.CancellationToken);
            }
            catch(OperationCanceledException ex)
            {
                await commonContext.UnSubscribeFromBook25UpdatesAsync(WriteToStreamAsync);
            }
            catch (Exception e)
            {
                //записываем ошибку в логер
                Log.Logger.Error(e.Message);
                //ставим статус "Отмена" в заголовке ответа
                context.Status = Status.DefaultCancelled;
            }
            finally
            {
                //отписываемся от обновлений по книге
                await commonContext.UnSubscribeFromBook25UpdatesAsync(WriteToStreamAsync);
            }

        }

    }
}
