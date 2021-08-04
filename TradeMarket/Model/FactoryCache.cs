using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex;

namespace TradeMarket.Model
{
    public class FactoryCache
    {
        private IConnectionMultiplexer _multiplexer;

        private IDictionary<string, TradeMarket> _tradeMarkets;

        public TradeMarketFactory(/*IConnectionMultiplexer multiplexer*/)
        {
            //_multiplexer = multiplexer;
            _tradeMarkets = new Dictionary<string, TradeMarket>(new List<KeyValuePair<string, TradeMarket>>
            {
                new KeyValuePair<string, TradeMarket>("bitmex",new BitmexTradeMarket("bitmex"/*,_multiplexer*/))
            });
        }


        public  TradeMarket GetTradeMarket(string name)
        {
            if (_tradeMarkets.ContainsKey(name))
            {
                return _tradeMarkets[name];
            }
            throw new ArgumentException($"{name} hasn't been implemented yet");
        }


        #region Static Part
        internal  List<UserContext> RegisteredUsers = new List<UserContext>();
        private object locker = new();

        public async Task<UserContext> GetUserContextAsync(string sessionId, string slotName, string tradeMarketName)
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
                    userContext = new UserContext(sessionId, slotName, GetTradeMarket(tradeMarketName));
                    //контекст сначала добавляется , а затеми инициализируется для того чтобы избежать создание нескольких контекстов
                    RegisteredUsers.Add(userContext);
                    userContext.init();
                }
                return userContext;
            }

        }
        #endregion
    }
}
