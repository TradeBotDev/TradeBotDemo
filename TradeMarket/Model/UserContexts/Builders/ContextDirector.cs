using Bitmex.Client.Websocket.Client;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Clients;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.Model.TradeMarkets;

namespace TradeMarket.Model.UserContexts.Builders
{
    public class ContextDirector
    {
        private readonly ContextBuilder _builder;
        private readonly AccountClient _accountClient;
        private readonly TradeMarketFactory _tradeMarketFactory;

        private readonly BitmexWebsocketClient _commonWSClient;

        private readonly RestfulClient _restClient;

        public ContextDirector(ContextBuilder builder, BitmexWebsocketClient wsClient, RestfulClient restClient, AccountClient accountClient, TradeMarketFactory tradeMarketFactory)
        {
            _builder = builder;
            _accountClient = accountClient;
            _tradeMarketFactory = tradeMarketFactory;

            //TODO убрать хардкод как-нибудь
            //TODO Сделать получение клиентов по конкретной бирже.Больше директоров !!!!!

            _commonWSClient = wsClient;
            _restClient = restClient;

        }



        private delegate Task<Context> BuildContextDeligate(string sessionId, string slotName, string tradeMarketName, CancellationToken token);


        internal async Task<Context> BuildContextAsync(
            string sessionId,
            string slotName,
            string tradeMarketName,
            CancellationToken token,
            Serilog.ILogger logger)
        {
            return await Task.Run(async () =>
            {
                var log = logger.ForContext<ContextDirector>().ForContext("Method", nameof(BuildContextAsync));
                var keySecretPair = await _accountClient.GetUserInfoAsync(sessionId);
                var tradeMarket = _tradeMarketFactory.GetTradeMarket(tradeMarketName);
                _builder
                    .AddUniqueInformation(slotName, sessionId)
                    .AddKeySecret(key: keySecretPair.Key, secret: keySecretPair.Secret)
                    .AddTradeMarket(tradeMarket);
                ;

                return await tradeMarket.BuildContextAsync(_builder, token, logger);
            });
        }

        internal async Task<Context> BuildCommonContextAsync(
            string sessioid, string slotName, string tradeMarketName, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                return new CommonContext();
                /*return new CommonContextBuilder(_builder.AddUniqueInformation(slotName, null))
                   .AddTradeMarket(_tradeMarketFactory.GetTradeMarket(tradeMarketName))
                   .Result;*/
            });
        }

        #region UserContext
        /// <summary>
        /// список пользователей которые уже вошли в сервис
        /// </summary>
        private List<Context> RegisteredUserContexts = new List<Context>();
        private readonly SemaphoreSlim _userContextSemaphore = new SemaphoreSlim(1);


        public async Task<Context> GetUserContextAsync(
            ContextFilter filter,
            CancellationToken token,
            ILogger logger)
        {
            var log = logger.ForContext<ContextDirector>().ForContext("Method", nameof(GetUserContextAsync));
            Context userContext = null;
            log.Debug("Locking {@Semephore}", _userContextSemaphore);
            await _userContextSemaphore.WaitAsync();
            try
            {
                //el.IsEquevalentTo(sessionId, slotName, tradeMarketName)
                Log.Logger.Information("Getting UserContext");
                userContext = RegisteredUserContexts.FirstOrDefault(filter.Func);
                Log.Logger.Information("Contained UserContext's count {@Count}", RegisteredUserContexts.Count);
                if (userContext is null)
                {
                    Log.Logger.Information("Creating new UserContext");
                    userContext = await BuildContextAsync(filter.SessionId, filter.SlotName, filter.TradeMarketName, token, log);
                    RegisteredUserContexts.Add(userContext);
                }
            }
            finally
            {
                log.Debug("Releasing {@Semephore}", _userContextSemaphore);
                _userContextSemaphore.Release();
            }
            log = log.ForContext("Context", userContext);
            try
            {
                log.Debug("Awaiting Autharization Task");
                return await userContext.AutharizationCompleted.Task;
            }
            catch
            {
                log.Warning("Autharization failer");
                RegisteredUserContexts.Remove(userContext);
                log.Warning("Removed context");
                throw;
            }
        }
        #endregion

    }
}
