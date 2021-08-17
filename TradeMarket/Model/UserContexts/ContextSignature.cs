using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Model.UserContexts
{
    public class ContextSignature
    {
        public string SlotName = "";
        public string TradeMarketName = "";
        public string SessionId = "";

        public ContextSignature() : this(null, null, null) { }


        public ContextSignature(string slotName, string tradeMarketName, string sessionId)
        {
            SlotName = slotName;
            TradeMarketName = tradeMarketName;
            SessionId = sessionId;
        }

        public static bool operator ==(ContextSignature left, ContextSignature right)
        {
            return EqualityComparer<ContextSignature>.Default.Equals(left, right);
        }

        public static bool operator !=(ContextSignature left, ContextSignature right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is ContextSignature signature &&
                   SlotName == signature.SlotName &&
                   TradeMarketName == signature.TradeMarketName &&
                   SessionId == signature.SessionId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SlotName, TradeMarketName, SessionId);
        }
    }
}
