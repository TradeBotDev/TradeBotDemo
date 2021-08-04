using Bitmex.Client.Websocket;
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

        internal UserContext BuildUserContext(string sessionId,string slotName,string tradeMarketName)
        {
            _builder.AddUniqueInformation(sessionId, slotName);

            var keySecretPair =  _accountClient.GetUserInfo(sessionId);
            return _builder
                .AddKeySecret(keySecretPair.Key, keySecretPair.Secret)
                //TODO Сделать получение клиентов по конкретной бирже.Больше директоров !!!!!
                .AddRestfulClient(new BitmexRestfulClient(BitmexRestufllLink.Testnet))
                .AddWebSocketClient(new BitmexWebsocketClient(new BitmexWebsocketCommunicator(BitmexValues.ApiWebsocketTestnetUrl)))
                .AddTradeMarket(_tradeMarketFactory.GetTradeMarket(tradeMarketName))
                .GetResult();
        }


        internal List<UserContext> RegisteredUsers = new List<UserContext>();
        private object locker = new();

        public UserContext GetUserContext(string sessionId, string slotName, string tradeMarketName)
        {
            UserContext userContext = null;
            lock (locker)
            {
                Log.Logger.Information("Getting UserContext {@sessionId} : {@slotName} : {@tradeMarketName}", sessionId, slotName, tradeMarketName);
                Log.Logger.Information("Stored Contexts : {@RegisteredUsers}", RegisteredUsers);
                userContext = RegisteredUsers.FirstOrDefault(el => el.IsEquevalentTo(sessionId, slotName, tradeMarketName));
                if (userContext is null)
                {
                    Log.Logger.Information("Creating new UserContext {@sessionId} : {@slotName} : {@tradeMarketName}", sessionId, slotName, tradeMarketName);
                    //TODO если uc нет в списке - создать новый через директора
                    userContext = BuildUserContext(sessionId, slotName, tradeMarketName);
                    RegisteredUsers.Add(userContext);
                }
                return userContext;
            }

        }
    }
}
