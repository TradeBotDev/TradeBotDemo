using System.Linq;
using System.Threading.Tasks;
using Former.Models;
using Grpc.Core;
using TradeBot.Former.FormerService.v1;

namespace Former.Services
{
    public class ListenerImpl : FormerService.FormerServiceBase
    {
        /// <summary>
        /// Данный метод запускает работу формера, так как срабатывает первым, то есть создаёт контекст и запускает подписки.
        /// Также в дальнейшем после запуска по созданому юзер контексту обновляется конфигурация.
        /// </summary>
        public override async Task<UpdateServerConfigResponse> UpdateServerConfig(UpdateServerConfigRequest request,
            ServerCallContext context)
        {
            var task = Task.Run(() =>
            {
                var meta = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
                //От релея приходят метаданные, по которым создаётся контекст 
                var userContext = Contexts.GetUserContext(meta["sessionid"], meta["trademarket"], meta["slot"]);
                //если поле Switch установленно в false, значит мы начинаем работу (или продолжаем её), если установлено в true
                //то необходимо остановить работу формера, то есть отписаться от трейдмаркета.
                if (request.Request.Switch) userContext.UnsubscribeStorage();
                else
                {
                    //устанавливаем или обновляем конфигурацию
                    userContext.SetConfiguration(Converters.ConvertConfiguration(request.Request.Config));
                    //подписываемся на трейдмаркет
                    userContext.SubscribeStorageToMarket();
                }
            });
            await task;
            return new UpdateServerConfigResponse();
        }

        /// <summary>
        /// Принимает запрос от релея на удаление всех своих ордеров (запрос выполняется по кнопке в UI).
        /// </summary>
        public override async Task<DeleteOrderResponse> DeleteOrder(DeleteOrderRequest request, ServerCallContext context)
        {
            var meta = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
            await Contexts.GetUserContext(meta["sessionid"], meta["trademarket"], meta["slot"]).RemoveAllMyOrders();
            return new DeleteOrderResponse();
        }

        /// <summary>
        /// Принимает решение от алгоритма, по которому необходимо будет выставить ордер.
        /// </summary>
        public override async Task<SendAlgorithmDecisionResponse> SendAlgorithmDecision(SendAlgorithmDecisionRequest request,
            ServerCallContext context)
        {
            var meta = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
            await Contexts.GetUserContext(meta["sessionid"], meta["trademarket"], meta["slot"]).FormOrder(request.Decision);
            return new SendAlgorithmDecisionResponse();
        }
    }
}
