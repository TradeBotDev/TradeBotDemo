﻿using System.Reactive.Subjects;
using Bitmex.Client.Websocket.Json;
using Bitmex.Client.Websocket.Messages;

namespace Bitmex.Client.Websocket.Responses.Trades
{
    /// <summary>
    /// Trades response
    /// </summary>
    public class TradeResponse : ResponseBase<Trade>
    {
        /// <summary>
        /// Operation type
        /// </summary>
        public override MessageType Op => MessageType.Trade;


        internal static bool TryHandle(string response, ISubject<TradeResponse> subject)
        {
            if (!BitmexJsonSerializer.ContainsValue(response, "trade"))
                return false;

            var parsed = BitmexJsonSerializer.Deserialize<TradeResponse>(response);
            subject.OnNext(parsed);

            return true;
        }
    }
}
