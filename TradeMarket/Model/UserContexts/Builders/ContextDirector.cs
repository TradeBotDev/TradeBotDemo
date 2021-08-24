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

        internal async Task<UserContext> BuildUserContextAsync(string sessionId, string slotName, string tradeMarketName,CancellationToken token)
        {
            return await Task.Run(async () =>
            {
                _builder.AddUniqueInformation(slotName,sessionId);
                var keySecretPair = await _accountClient.GetUserInfoAsync(sessionId);
                var userContextBuilder = new UserContextBuilder(_builder);
                userContextBuilder
                .AddKeySecret(key:keySecretPair.Key,secret: keySecretPair.Secret)
                .AddWebSocketClient(_commonWSClient)
                .AddTradeMarket(_tradeMarketFactory.GetTradeMarket(tradeMarketName));
                return await userContextBuilder.InitUser(token);
            });
        }

        internal async Task<CommonContext> BuildCommonContextAsync(string sessioid,string slotName,string tradeMarketName,CancellationToken token)
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


        public async Task<UserContext> GetUserContextAsync(ContextFilter filter, CancellationToken token)
        {
            UserContext userContext = null;
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
                    userContext = await BuildUserContextAsync(filter.SessionId, filter.SlotName, filter.TradeMarketName,token);
                    RegisteredUserContexts.Add(userContext);
                }
            }
            finally
            {
                _userContextSemaphore.Release();
            }
            try
            {
                return await userContext.AutharizationCompleted.Task;
            }
            catch (Exception e)
            {
                RegisteredUserContexts.Remove(userContext);
                throw;
            }
        }
        #endregion
       
    }
}
