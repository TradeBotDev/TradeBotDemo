using Grpc.Core;
using Serilog;
using System;
using System.Threading.Tasks;
using TradeBot.TradeMarket.TradeMarketService.v1;
using TradeMarket.Model.UserContexts;

namespace TradeMarket.Services
{
    public partial class TradeMarketService : TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceBase
    {
        private ILogger logger => Log.Logger.ForContext("Type", nameof(TradeMarketService));

        /// <summary>
        /// Размещает ордер на выбранной бирже
        /// </summary>
        public async override Task<TradeBot.TradeMarket.TradeMarketService.v1.PlaceOrderResponse> PlaceOrder(TradeBot.TradeMarket.TradeMarketService.v1.PlaceOrderRequest request, ServerCallContext context)
        {
            UserContext user = null;
            try
            {
                //ищем конеткст пользователя
                user = await GetUserContextAsync(context.RequestHeaders);
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
                var user = await GetUserContextAsync(context.RequestHeaders);

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
                user = await GetUserContextAsync(context.RequestHeaders);

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
}
