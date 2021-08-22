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

    class FilterParameter
    {
        public string Value { get; set; }
        public bool ShouldBeUsed { get; set; }

        public FilterParameter(string value, bool shouldBeUsed)
        {
            Value = value;
            ShouldBeUsed = shouldBeUsed;
        }
    }

    public class ContextFilter
    {
        public Func<UserContext, bool> Filter { get; }
        public ContextFilterType Type { get; }

        private FilterParameter _sessionId { get; }
        private FilterParameter _slotName { get; }
        private FilterParameter _tradeMarketName { get; }

        public string SessionId
        {
            get
            {
                return _sessionId.Value;
                //return _sessionId.ShouldBeUsed ? _sessionId.Value : null;
            }
        }
        
        public string SlotName
        {
            get
            {
                return _slotName.Value;
                //return _slotName.ShouldBeUsed ? _slotName.Value : null;
            }
        }

        public string TradeMarketName
        {
            get
            {
                return _tradeMarketName.Value;
                //return _tradeMarketName.ShouldBeUsed ? _tradeMarketName.Value : null;
            }
        }

        private ContextFilter(FilterParameter sessionId, FilterParameter slotName, FilterParameter tradeMarketName, ContextFilterType type)
        {
            Type = type;
            _sessionId = sessionId;
            _slotName = slotName;
            _tradeMarketName = tradeMarketName;
        }

        public Func<UserContext, bool> Func { 
            get => 
                (context) => context.IsEquevalentTo(
                    sessionId: _sessionId.ShouldBeUsed ? SessionId : null, 
                    slotName: _slotName.ShouldBeUsed ? SlotName : null,
                    tradeMarketName: _tradeMarketName.ShouldBeUsed ? TradeMarketName : null); 
        }

        public delegate ContextFilter GetFilter(string sessionId, string slotName, string tradeMarketName);
        public static ContextFilter GetFullContextFilter(string sessionId, string slotName,string tradeMarketName)=> new( new(sessionId,true), new(slotName,true), new(tradeMarketName,true), ContextFilterType.Full);
        public static ContextFilter GetCommonContextFilter(string sessionId, string slotName, string tradeMarketName) => new(new(sessionId,false), new(slotName, true), new(tradeMarketName, true), ContextFilterType.Common);
        public static ContextFilter GetTradeMarketContextFilter(string sessionId, string slotName, string tradeMarketName) => new(new(sessionId,false), new(slotName,false), new(tradeMarketName,true), ContextFilterType.TradeMarket);


    }
}
