﻿using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Websockets;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.Clients;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.Model.TradeMarkets;

namespace TradeMarket.Model.UserContexts
{
    public class UserContextDirector
    {
        private UserContextBuilder _builder;

        private readonly AccountClient _accountClient;
        private readonly TradeMarketFactory _tradeMarketFactory;

        public UserContextDirector(UserContextBuilder builder, AccountClient accountClient,TradeMarketFactory tradeMarketFactory)
        {
            _builder = builder;
            _accountClient = accountClient;
            _tradeMarketFactory = tradeMarketFactory;
        }

        internal async Task<UserContext> BuildUserContext(string sessionId,string slotName,string tradeMarketName)
        {
            var keySecretPair =  await _accountClient.GetUserInfoAsync(sessionId);
            return _builder
                .AddUniqueInformation(sessionId, slotName)
                .AddKeySecret(keySecretPair.Key, keySecretPair.Secret)
                //TODO Сделать получение клиентов по конкретной бирже.Больше директоров !!!!!
                .AddRestfulClient(new BitmexRestfulClient(BitmexRestufllLink.Testnet))
                .AddWebSocketClient(new BitmexWebsocketClient(new BitmexWebsocketCommunicator(BitmexValues.ApiWebsocketTestnetUrl)))
                .AddTradeMarket(_tradeMarketFactory.GetTradeMarket(tradeMarketName))
                .InitUser()
                .GetResult();
        }


        internal List<UserContext> RegisteredUsers = new List<UserContext>();
        private object locker = new();

        public async Task<UserContext> GetUserContextAsync(string sessionId, string slotName, string tradeMarketName)
        {
            bool CreatingUserContextIsRequired = false;
            UserContext userContext = null;
            lock (locker)
            {
                Log.Logger.Information("Getting UserContext {@sessionId} : {@slotName} : {@tradeMarketName}", sessionId, slotName, tradeMarketName);
                userContext = RegisteredUsers.FirstOrDefault(el => el.IsEquevalentTo(sessionId, slotName, tradeMarketName));
                if (userContext is null)
                {
                    Log.Logger.Information("Creating new UserContext {@sessionId} : {@slotName} : {@tradeMarketName}", sessionId, slotName, tradeMarketName);
                    //TODO если uc нет в списке - создать новый через директора
                    CreatingUserContextIsRequired = true;
                }
            }
            if (CreatingUserContextIsRequired)
            {
                //дополнительная проверка после локера
                userContext = RegisteredUsers.FirstOrDefault(el => el.IsEquevalentTo(sessionId, slotName, tradeMarketName));
                if (userContext is null)
                {
                    userContext = await BuildUserContext(sessionId, slotName, tradeMarketName);
                    RegisteredUsers.Add(userContext);
                }
            }
            return userContext;

        }
    }
}
