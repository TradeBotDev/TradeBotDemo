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

        public ContextDirector(ContextBuilder builder, AccountClient accountClient, TradeMarketFactory tradeMarketFactory)
        {
            _builder = builder;
            _accountClient = accountClient;
            _tradeMarketFactory = tradeMarketFactory;
            //TODO убрать хардкод как-нибудь
            //TODO Сделать получение клиентов по конкретной бирже.Больше директоров !!!!!

            _commonWSClient = GetNewBitmexWebSocketClient();
            _restClient = new BitmexRestfulClient(BitmexRestufllLink.Testnet);
        }

        public BitmexWebsocketClient GetNewBitmexWebSocketClient()
        {
            return new BitmexWebsocketClient(new BitmexWebsocketCommunicator(BitmexValues.ApiWebsocketTestnetUrl));
        }

        internal async Task<UserContext> BuildUserContextAsync(string sessionId, string slotName, string tradeMarketName,CancellationToken token)
        {
            return await Task.Run(async () =>
            {
                _builder.AddUniqueInformation(sessionId, slotName);
                var keySecretPair = await _accountClient.GetUserInfoAsync(sessionId);
                var userContextBuilder = new UserContextBuilder(_builder);
                userContextBuilder
                .AddKeySecret(keySecretPair.Key, keySecretPair.Secret)
                .AddWebSocketClient(GetNewBitmexWebSocketClient())
                .AddTradeMarket(_tradeMarketFactory.GetTradeMarket(tradeMarketName));
                return await userContextBuilder.InitUser(token);
            });
        }

        internal async Task<CommonContext> BuildCommonContextAsync(string slotName,string tradeMarketName)
        {
            return await Task.Run(() =>
            {
                _builder.AddUniqueInformation(slotName, null);
                return new CommonContextBuilder(_builder)
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

        public async Task<UserContext> GetUserContextAsync(string sessionId, string slotName, string tradeMarketName,CancellationToken token)
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
                    userContext = await BuildUserContextAsync(sessionId, slotName, tradeMarketName,token);
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
