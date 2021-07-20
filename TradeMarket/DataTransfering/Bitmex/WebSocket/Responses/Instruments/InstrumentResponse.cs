using System;
using System.Reactive.Subjects;
using Bitmex.Client.Websocket.Json;
using Bitmex.Client.Websocket.Messages;

namespace Bitmex.Client.Websocket.Responses.Instruments
{
    public class InstrumentResponse : ResponseBase<Instrument>
    {
        /// <inheritdoc />
        public override MessageType Op => MessageType.Instrument;

       

        internal static bool TryHandle(string response, ISubject<InstrumentResponse> subject)
        {
            if (!BitmexJsonSerializer.ContainsValue(response, "instrument"))
                return false;

            var parsed = BitmexJsonSerializer.Deserialize<InstrumentResponse>(response);
            subject.OnNext(parsed);

            return true;
        }
    }
}
