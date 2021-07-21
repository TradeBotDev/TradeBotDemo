using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.Model;
namespace TradeMarket.DataTransfering.Bitmex
{
    public class BitmexUserContext
    {
        public String SessionId { get; set; }
        public String Key { get; set; }
        public String Secret { get; set; }

        internal BitmexWebsocketClient WSClient { get; set; }
        internal BitmexRestfulClient RestClient { get; set; }

        public Model.TradeMarket TradeMarket { get; set; }

        public event EventHandler<FullOrder> Book25;
        public event EventHandler<FullOrder> Book;
        public event EventHandler<FullOrder> UserOrders;
        public event EventHandler<Model.Balance> UserBalance;

        public BitmexUserContext(string sessionId)
        {
            //инициализация websocket клиента
            var communicator = new BitmexWebsocketCommunicator(BitmexValues.ApiWebsocketUrl);
            WSClient = new BitmexWebsocketClient(communicator);
            communicator.Start();

            //инициализация http клиента
            RestClient = new BitmexRestfulClient();

            this.SessionId = sessionId;
            //TODO обращение к сервису авторизации для получения ключа и секретки
            this.Key = "lVjs4QoJIe9OqNUnDoVKl2jS";
            this.Secret = "MAX6lma-Y93bfT3w-g5GtAgvsFNhDLrYlyyqkciDUwRTy64s";

            //инициализация подписок
            TradeMarket.SubscribeToBalance((sender,el) => { UserBalance?.Invoke(sender,el); }, this);
            TradeMarket.SubscribeToBook((sender, el) => { Book?.Invoke(sender, el); }, this);
            TradeMarket.SubscribeToBook25((sender, el) => Book25?.Invoke(sender, el),this);
            TradeMarket.SubscribeToUserOrders((sender, el) => UserOrders?.Invoke(sender, el), this);

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
