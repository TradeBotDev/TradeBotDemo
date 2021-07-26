using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeMarket.Clients;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;

namespace TradeMarket.Model
{
    public class UserContext
    {
        #region Dynamic Part
        public String SessionId { get; set; }
        public String SlotName { get; set; }

        public String Key { get; set; }
        public String Secret { get; set; }

        public Model.TradeMarket TradeMarket { get; set; }

        public event EventHandler<FullOrder> Book25;
        public event EventHandler<FullOrder> Book;
        public event EventHandler<FullOrder> UserOrders;
        public event EventHandler<Model.Balance> UserBalance;

        //TODO сделать эти классы абстрактными
        internal BitmexWebsocketClient WSClient { get; set; }
        internal BitmexRestfulClient RestClient { get; set; }

        private AccountClient _accountClient;

        private async void init()
        {
            var keySecretPair = await _accountClient.GetUserInfo(SessionId).ContinueWith(el => {
                Key = el.Result.Key;
                Secret = el.Result.Secret;
                return AutheticateUser();
            } );

            //инициализация подписок
            TradeMarket.SubscribeToBalance((sender, el) => { UserBalance?.Invoke(sender, el); }, this);
            TradeMarket.SubscribeToBook((sender, el) => { Book?.Invoke(sender, el); }, this);
            TradeMarket.SubscribeToBook25((sender, el) => Book25?.Invoke(sender, el), this);
            TradeMarket.SubscribeToUserOrders((sender, el) => UserOrders?.Invoke(sender, el), this);
        }


        internal UserContext(string sessionId, string slotName, Model.TradeMarket tradeMarket)
        {
            //инициализация websocket клиента
            var communicator = new BitmexWebsocketCommunicator(BitmexValues.ApiWebsocketUrl);
            WSClient = new BitmexWebsocketClient(communicator);
            communicator.Start();

            //инициализация http клиента
            RestClient = new BitmexRestfulClient();

            SessionId = sessionId;
            SlotName = slotName;

            TradeMarket = tradeMarket;
            _accountClient = AccountClient.GetInstance();
            init();

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
            return await TradeMarket.AutheticateUser(Key, Secret, this);
        }

        public bool IsEquevalentTo(string sessionId, string slotName, string tradeMarketName)
        {
            return this.SessionId == sessionId && this.SlotName == slotName && this.TradeMarket.Name == tradeMarketName;
        }
        #endregion

        #region Static Part
        internal static List<UserContext> RegisteredUsers = new List<UserContext>();

        public static UserContext GetUserContext(string sessionId, string slotName)
        {
            if (RegisteredUsers.FirstOrDefault(el => el.IsEquevalentTo(sessionId, slotName, "Bitmex")) is null)
            {
                RegisterUser(sessionId, slotName, "Bitmex");
            }
            return RegisteredUsers.First(el => el.IsEquevalentTo(sessionId, slotName, "Bitmex"));
        }

        public static void RegisterUser(string sessionId, string slotName, string tradeMarketName) 
        {
            UserContext user = new UserContext(sessionId, slotName, TradeMarket.GetTradeMarket(tradeMarketName));

            RegisteredUsers.Add(user);
        }
        #endregion
    }
}

