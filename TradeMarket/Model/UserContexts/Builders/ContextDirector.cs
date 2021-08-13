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
        private UserContextBuilder _userContextBuilder;
        private readonly CommonContextBuilder _commonContextBuilder;
        private readonly AccountClient _accountClient;
        private readonly TradeMarketFactory _tradeMarketFactory;

        private readonly BitmexWebsocketClient _wsClient;

        private readonly BitmexRestfulClient _restClient;

        public ContextDirector(UserContextBuilder userContextBuilder,CommonContextBuilder commonContextBuilder, AccountClient accountClient, TradeMarketFactory tradeMarketFactory)
        {
            _userContextBuilder = userContextBuilder;
            _commonContextBuilder = commonContextBuilder;
            _accountClient = accountClient;
            _tradeMarketFactory = tradeMarketFactory;
            //TODO убрать хардкод как-нибудь
            //TODO Сделать получение клиентов по конкретной бирже.Больше директоров !!!!!

            _wsClient = new BitmexWebsocketClient(new BitmexWebsocketCommunicator(BitmexValues.ApiWebsocketTestnetUrl));
            _restClient = new BitmexRestfulClient(BitmexRestufllLink.Testnet);
        }

        internal async Task<UserContext> BuildUserContextAsync(string sessionId, string slotName, string tradeMarketName)
        {
            var keySecretPair = await _accountClient.GetUserInfoAsync(sessionId);
            return await Task.FromResult(
                _userContextBuilder
                .AddUniqueInformation(sessionId, slotName)
                .AddKeySecret(keySecretPair.Key, keySecretPair.Secret)
                .AddWebSocketClient(_wsClient)
                .AddTradeMarket(_tradeMarketFactory.GetTradeMarket(tradeMarketName))
                .InitUser()
                .Result
                );
        }

        internal async Task<CommonContext> BuildCommonContextAsync(string slotName,string tradeMarketName)
        {
            return await Task.FromResult(
                _commonContextBuilder
                .AddUniqueInformation(slotName)
                .AddTradeMarket(_tradeMarketFactory.GetTradeMarket(tradeMarketName))
                .Result);
        }

        #region UserContext
        /// <summary>
        /// список пользователей которые уже вошли в сервис
        /// </summary>
        private List<UserContext> RegisteredUserContexts = new List<UserContext>();
        private readonly SemaphoreSlim _userContextSemaphore = new SemaphoreSlim(1);

        public async Task<UserContext> GetUserContextAsync(string sessionId, string slotName, string tradeMarketName)
        {
            UserContext userContext = null;
            await _userContextSemaphore.WaitAsync();
            try
            {
                Log.Logger.Information("Getting UserContext {@sessionId} : {@slotName} : {@tradeMarketName}", sessionId, slotName, tradeMarketName);
                userContext = RegisteredUserContexts.FirstOrDefault(el => el.IsEquevalentTo(sessionId, slotName, tradeMarketName));
                Log.Logger.Information("Contained UserContext's count {@Count}", RegisteredUserContexts.Count);
                if (userContext is  null)
                {
                    Log.Logger.Information("Creating new UserContext {@sessionId} : {@slotName} : {@tradeMarketName}", sessionId, slotName, tradeMarketName);
                    userContext = await BuildUserContextAsync(sessionId, slotName, tradeMarketName);
                    RegisteredUserContexts.Add(userContext);
                }
            }
            finally
            {
                _userContextSemaphore.Release();
            }

            return userContext;
        }
        #endregion
        #region CommonContext
        private List<CommonContext> RegisteredCommonContexts = new List<CommonContext>();
        private readonly SemaphoreSlim _commonContextSemephore = new SemaphoreSlim(1);

        public async Task<CommonContext> GetCommonContextAsync(string slotName,string trareMarketName)
        {
            CommonContext commonContext = null;
            await _commonContextSemephore.WaitAsync();
            try
            {
                commonContext = RegisteredCommonContexts.FirstOrDefault(el => el.IsEquevalentTo(null, slotName, trareMarketName));
                if(commonContext is null)
                {
                    commonContext = await BuildCommonContextAsync(slotName, trareMarketName);
                    RegisteredCommonContexts.Add(commonContext);
                }
            }
            finally
            {
                _commonContextSemephore.Release();
            }
            return commonContext;
        }

        #endregion
    }
}
