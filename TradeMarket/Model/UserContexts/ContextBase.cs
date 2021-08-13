using Bitmex.Client.Websocket.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Model.UserContexts
{
    public abstract class ContextBase : IContext
    {
        public ContextSignature Signature { get; set; }

        public string Key { get; set; } = null;

        public string Secret { get; set; } = null;


        public ContextBase()
        {
            Signature = null;
        }
        public ContextBase(ContextSignature signature)
        {
            Signature = signature;
        }

        public bool IsEquevalentTo(string sessionId, string slotName, string tradeMarketName)
        {
            return Signature.SessionId == sessionId && Signature.SlotName == slotName && Signature.TradeMarketName == tradeMarketName;
        }

        public static bool operator == (ContextBase left,ContextBase right)
        {
            return left.Signature == right.Signature;
        }

        public static bool operator !=(ContextBase left, ContextBase right)
        {
            return left.Signature != right.Signature;
        }

        public override bool Equals(object obj)
        {
            return obj is ContextBase && this.Equals(obj as ContextBase);
                   
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Signature);
        }

        public bool Equals(IContext other)
        {
            return EqualityComparer<ContextSignature>.Default.Equals(Signature, other.Signature);
        }
    }
}
