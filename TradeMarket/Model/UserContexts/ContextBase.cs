using Bitmex.Client.Websocket.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Model.UserContexts
{
    public class ContextBase : IContext ,ICloneable
    {
        public ContextSignature Signature { get; set; }

        public string Key { get; set; } = null;

        public string Secret { get; set; } = null;

        public ContextBase(IContext other) :this()
        {
            Signature.SessionId = other.Signature.SessionId;
            Signature.SlotName = other.Signature.SlotName;
            Signature.TradeMarketName = other.Signature.TradeMarketName;
            Key = other.Key;
            Secret = other.Secret;
        }

        public ContextBase() : this(new ContextSignature())
        {

        }
        public ContextBase(ContextSignature signature)
        {
            Signature = signature;
        }

        public bool IsEquevalentTo(string sessionId, string slotName, string tradeMarketName)
        {
            bool sessionCheck = Signature.SessionId == sessionId || sessionId == null;
            bool slotNameCheck = Signature.SlotName == slotName || slotName == null;
            bool tradeMarketNameCheck = Signature.TradeMarketName == tradeMarketName || tradeMarketName == null;
            return sessionCheck && slotNameCheck &&  tradeMarketNameCheck;
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

        public object Clone()
        {
            return new ContextBase(this);
        }
    }
}
