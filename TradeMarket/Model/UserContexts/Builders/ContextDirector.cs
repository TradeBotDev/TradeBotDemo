using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Websockets;
using Serilog;
using System;
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

        private readonly BitmexRestfulClient _restClient;

        public ContextDirector(ContextBuilder builder,BitmexWebsocketClient wsClient,BitmexRestfulClient restClient, AccountClient accountClient, TradeMarketFactory tradeMarketFactory)
        {
            _builder = builder;
            _accountClient = accountClient;
            _tradeMarketFactory = tradeMarketFactory;

            //TODO убрать хардкод как-нибудь
            //TODO Сделать получение клиентов по конкретной бирже.Больше директоров !!!!!

            _commonWSClient = wsClient;
            _restClient = restClient;

        }

        public BitmexWebsocketClient GetNewBitmexWebSocketClient()
        {
            return new BitmexWebsocketClient(new BitmexWebsocketCommunicator(BitmexValues.ApiWebsocketTestnetUrl));
        }

        private delegate Task<UserContext> BuildContextDeligate(string sessionId,string slotName,string tradeMarketName,CancellationToken token);

        internal BitmexWebsocketClient CreateWebsocketClient(ILogger logger)
        {
            var log = logger.ForContext("Method", nameof(CreateWebsocketClient));
            logger.Information("Creating new BitmexWebsocketClient");
            var communicator = new BitmexWebsocketCommunicator(BitmexValues.ApiWebsocketTestnetUrl);
            var res = new BitmexWebsocketClient(communicator);
            communicator.Start();
            return res;
        }

        internal async Task<UserContext> BuildUserContextAsync(
            string sessionId, 
            string slotName, 
            string tradeMarketName,
            CancellationToken token,
            Serilog.ILogger logger)
        {
            return await Task.Run(async () =>
            {
                var log = logger.ForContext("Method", nameof(BuildUserContextAsync));
                _builder.AddUniqueInformation(slotName,sessionId);
                var keySecretPair = await _accountClient.GetUserInfoAsync(sessionId);
                var userContextBuilder = new UserContextBuilder(_builder);
                userContextBuilder
                .AddKeySecret(key:keySecretPair.Key,secret: keySecretPair.Secret)
                .AddWebSocketClient(CreateWebsocketClient(log))
                .AddTradeMarket(_tradeMarketFactory.GetTradeMarket(tradeMarketName));
                return await userContextBuilder.InitUser(token,log);
            });
        }

        internal async Task<CommonContext> BuildCommonContextAsync(
            string sessioid,string slotName,string tradeMarketName,CancellationToken token)
        {
            return await Task.Run(() =>
            {
                return new CommonContextBuilder(_builder.AddUniqueInformation(slotName, null))
                   .AddTradeMarket(_tradeMarketFactory.GetTradeMarket(tradeMarketName))
                   .Result;
            });
        }

        #region UserContext
        /// <summary>
        /// список пользователей которые уже вошли в сервис
        /// </summary>
        private List<UserContext> RegisteredUserContexts = new List<UserContext>();
        private readonly SemaphoreSlim _userContextSemaphore = new SemaphoreSlim(1);


        public async Task<UserContext> GetUserContextAsync(
            ContextFilter filter, 
            CancellationToken token,
            ILogger logger)
        {
            var log = logger.ForContext<ContextDirector>().ForContext("Method", nameof(GetUserContextAsync));
            UserContext userContext = null;
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
                    userContext = await BuildUserContextAsync(filter.SessionId, filter.SlotName, filter.TradeMarketName,token,log);
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
