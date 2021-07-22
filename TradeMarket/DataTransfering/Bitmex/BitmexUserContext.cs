using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Websockets;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.Model;
namespace TradeMarket.DataTransfering.Bitmex
{
    public class BitmexUserContext
    {
        public String SessionId { get; set; }
        public String SlotName { get; set; }

        public String Key { get; set; }
        public String Secret { get; set; }

        internal BitmexWebsocketClient WSClient { get; set; }
        internal BitmexRestfulClient RestClient { get; set; }

        public Model.TradeMarket TradeMarket { get; set; }

        public event EventHandler<FullOrder> Book25;
        public event EventHandler<FullOrder> Book;
        public event EventHandler<FullOrder> UserOrders;
        public event EventHandler<Model.Balance> UserBalance;

        public BitmexUserContext(string sessionId,string slotName,Model.TradeMarket tradeMarket)
        {
            //инициализация websocket клиента
            var communicator = new BitmexWebsocketCommunicator(BitmexValues.ApiWebsocketUrl);
            WSClient = new BitmexWebsocketClient(communicator);
            communicator.Start();

            //инициализация http клиента
            RestClient = new BitmexRestfulClient();

            SessionId = sessionId;
            SlotName = slotName;
            //TODO обращение к сервису авторизации для получения ключа и секретки
            Key = "lVjs4QoJIe9OqNUnDoVKl2jS";
            Secret = "MAX6lma-Y93bfT3w-g5GtAgvsFNhDLrYlyyqkciDUwRTy64s";

            TradeMarket = tradeMarket;
            AutheticateUser();

            //инициализация подписок
            TradeMarket.SubscribeToBalance((sender,el) => { UserBalance?.Invoke(sender,el); }, this);
            TradeMarket.SubscribeToBook((sender, el) => { Book?.Invoke(sender, el); }, this);
            TradeMarket.SubscribeToBook25((sender, el) => Book25?.Invoke(sender, el),this);
            TradeMarket.SubscribeToUserOrders((sender, el) => UserOrders?.Invoke(sender, el), this);

            TradeMarket.Book25Update += TradeMarket_Book25Update;

        }

        private void TradeMarket_Book25Update(object sender, FullOrder e)
        {
            Book25?.Invoke(sender, e);
        }

        public async Task<DefaultResponse> PlaceOrder(double quontity, double price)
        {
            return await TradeMarket.PlaceOrder(quontity, price, this);
        }

        public async Task<DefaultResponse> CloseOrder(string id)
        {
            return await TradeMarket.CloseOrder(id, this);
        }

        public async Task<DefaultResponse> AutheticateUser()
        {
            return await TradeMarket.AutheticateUser(Key, Secret,this);
        }


 


    }
}
