using Bitmex.Client.Websocket.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Model.UserContexts
{
    public interface IContext: IEquatable<IContext>
    {
        public ContextSignature Signature { get; set; }

        public string Key { get; set; }

        public string Secret { get; set; }

        public bool IsEquevalentTo(string sessionId, string slotName, string tradeMarketName);
    }
}
