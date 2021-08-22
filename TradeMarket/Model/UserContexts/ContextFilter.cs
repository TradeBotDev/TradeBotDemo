using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Model.UserContexts
{
    public enum ContextFilterType
    {
        Full,
        Common,
        TradeMarket
    }

    public class ContextFilter
    {
        public Func<UserContext, bool> Filter { get; }
        public ContextFilterType Type { get; }

        public string SessionId { get; }
        public string SlotName { get; }
        public string TradeMarketName { get; }

        private ContextFilter(string sessionId,string slotName,string tradeMarketName, ContextFilterType type)
        {
            Type = type;
            SessionId = sessionId;
            SlotName = slotName;
            TradeMarketName = tradeMarketName;
        }

        public Func<UserContext, bool> Func { get => (context) => context.IsEquevalentTo(sessionId: SessionId, slotName: SlotName, tradeMarketName: TradeMarketName); }

        public delegate ContextFilter GetFilter(string sessionId, string slotName, string tradeMarketName);
        public static ContextFilter GetFullContextFilter(string sessionId, string slotName,string tradeMarketName)=> new( sessionId, slotName, tradeMarketName, ContextFilterType.Full);
        public static ContextFilter GetCommonContextFilter(string sessionId, string slotName, string tradeMarketName) => new(null, slotName, tradeMarketName, ContextFilterType.Common);
        public static ContextFilter GetTradeMarketContextFilter(string sessionId, string slotName, string tradeMarketName) => new(null, null, tradeMarketName, ContextFilterType.TradeMarket);


    }
}
