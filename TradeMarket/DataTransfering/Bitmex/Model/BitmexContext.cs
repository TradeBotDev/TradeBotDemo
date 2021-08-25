using Bitmex.Client.Websocket.Client;
using TradeMarket.Model.UserContexts;

namespace TradeMarket.DataTransfering.Bitmex.Model
{
    public class BitmexContext : Context
    {
        //Клиенты для доступа к личной информации пользователя на бирже
        internal BitmexWebsocketClient WSClient { get; set; }

        public BitmexContext(Context context): base(context)
        {

        }

        public BitmexContext():base()
        {

        }

    }
}

