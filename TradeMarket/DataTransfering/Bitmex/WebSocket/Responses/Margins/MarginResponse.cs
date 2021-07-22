using System.Reactive.Subjects;
using Bitmex.Client.Websocket.Json;
using Bitmex.Client.Websocket.Messages;

namespace Bitmex.Client.Websocket.Responses.Margins
{
    public class MarginResponse : ResponseBase<Margin>
    {
        public override MessageType Op => MessageType.Margin;

        internal static bool TryHandle(string response, ISubject<MarginResponse> subject)
        {
            if (!BitmexJsonSerializer.ContainsValue(response, "margin"))
                return false;

            var parsed = BitmexJsonSerializer.Deserialize<MarginResponse>(response);
            subject.OnNext(parsed);

            return true;
        }
    }
}
