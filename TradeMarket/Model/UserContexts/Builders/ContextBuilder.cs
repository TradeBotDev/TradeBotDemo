using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Model.UserContexts.Builders
{
    public abstract class ContextBuilder
    {
        internal ContextBase Context;

        public ContextBuilder() { Context = new ContextBase(); }

        public ContextBuilder AddUniqueInformation(string slotName, string sessionId)
        {
            Context.Signature.SessionId = sessionId;
            Context.Signature.SlotName = slotName;
            return this;
        }
    }
}
