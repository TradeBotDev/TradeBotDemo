using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Model.UserContexts.Builders
{
    public class ContextBuilder
    {
        private ContextBase _context;
        internal ContextBase Context { 
            get
            {
                var result = _context.Clone() as ContextBase;
                _context = new ContextBase();
                return result;
            }
            set
            {
                _context = value;
            }
        }

        public ContextBuilder() { Context = new ContextBase(); }

        public ContextBuilder AddUniqueInformation(string slotName, string sessionId)
        {
            _context.Signature.SessionId = sessionId;
            _context.Signature.SlotName = slotName;
            return this;
        }
    }
}
