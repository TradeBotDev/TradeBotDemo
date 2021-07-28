using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Websockets;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;
using TradeMarket.Clients;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;

namespace TradeMarket.Model
{
    public class UserContext : IEquatable<UserContext>
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

        /// <summary>
        /// Метод инициализации контекста. 
        /// Публичный из-за бага: Когда нет объектов в списке RegisteredUsers и создается новый контекст он создается так долго, что при новом запросе на контекст старый не успевает создасться и добавиться в список и создается еще один контекст.
        /// </summary>
        public async Task initAsync()
        {
            var keySecretPair = await _accountClient.GetUserInfo(SessionId).ContinueWith(el => {
                try
                {
                    Key = el.Result.Key;
                    Secret = el.Result.Secret;
                    return AutheticateUser();
                }
                catch (Exception e)
                {
                    Log.Logger.Error($"Exception: {e.Message}");
                }
                return Task.Delay(0);

            });

            //инициализация подписок
            TradeMarket.SubscribeToBalance((sender, el) => { UserBalance?.Invoke(sender, el); }, this);
            TradeMarket.SubscribeToBook((sender, el) => { Book?.Invoke(sender, el); }, this);
            TradeMarket.SubscribeToBook25((sender, el) => Book25?.Invoke(sender, el), this);
            TradeMarket.SubscribeToUserOrders((sender, el) => UserOrders?.Invoke(sender, el), this);
        }

        /// <summary>
        /// После создание нового объекта необходима инициализация некоторых полей и ивентов через метод init()
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="slotName"></param>
        /// <param name="tradeMarket"></param>
        internal UserContext(string sessionId, string slotName, Model.TradeMarket tradeMarket)
        {
            //инициализация websocket клиента
            var communicator = new BitmexWebsocketCommunicator(BitmexValues.ApiWebsocketTestnetUrl);
            WSClient = new BitmexWebsocketClient(communicator);
            communicator.Start();

            //инициализация http клиента
            RestClient = new BitmexRestfulClient();

            SessionId = sessionId;
            SlotName = slotName;

            TradeMarket = tradeMarket;
            _accountClient = AccountClient.GetInstance();
        }

        public async Task<PlaceOrderResponse> PlaceOrder(double quontity, double price)
        {
            return await TradeMarket.PlaceOrder(quontity, price, this);
        }

        public async Task<DefaultResponse> DeleteOrder(string id)
        {
            return await TradeMarket.DeleteOrder(id, this);
        }

        public async Task<DefaultResponse> AmmendOrder(string id, double? price, long? Quantity, long? LeavesQuantity)
        {
            return await TradeMarket.AmmendOrder(id, price, Quantity, LeavesQuantity, this);
        }

        public async Task<DefaultResponse> AutheticateUser()
        {
            return await TradeMarket.AutheticateUser(Key, Secret, this);
        }

        public bool IsEquevalentTo(string sessionId, string slotName, string tradeMarketName)
        {
            return this.SessionId == sessionId && this.SlotName == slotName && this.TradeMarket.Name == tradeMarketName;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as UserContext);
        }

        public bool Equals(UserContext other)
        {
            return other != null &&
                   SessionId == other.SessionId &&
                   SlotName == other.SlotName &&
                   TradeMarket.Name == other.TradeMarket.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SessionId, SlotName, TradeMarket.Name);
        }

        public static bool operator ==(UserContext left, UserContext right)
        {
            return EqualityComparer<UserContext>.Default.Equals(left, right);
        }

        public static bool operator !=(UserContext left, UserContext right)
        {
            return !(left == right);
        }

        #endregion

        #region Static Part
        internal static List<UserContext> RegisteredUsers = new List<UserContext>();
        private static object locker = new();

        public static async Task<UserContext> GetUserContextAsync(string sessionId, string slotName, string tradeMarketName)
        {
            UserContext userContext = null;
            try
            {
                System.Threading.Monitor.Enter(locker);
                Log.Logger.Information("Getting UserContext {@sessionId} : {@slotName} : {@tradeMarketName}", sessionId, slotName, tradeMarketName);
                Log.Logger.Information("Stored Contexts : {@RegisteredUsers}", RegisteredUsers);
                userContext = RegisteredUsers.FirstOrDefault(el => el.IsEquevalentTo(sessionId, slotName, "bitmex"));
                if (userContext is null)
                {
                    Log.Logger.Information("Creating new UserContext {@sessionId} : {@slotName} : {@tradeMarketName}", sessionId, slotName, tradeMarketName);
                    userContext = new UserContext(sessionId, slotName, TradeMarket.GetTradeMarket(tradeMarketName));
                    //контекст сначала добавляется , а затеми инициализируется для того чтобы избежать создание нескольких контекстов
                    RegisteredUsers.Add(userContext);
                    await userContext.initAsync();
                }
            } finally {
                System.Threading.Monitor.Exit(locker);
            }
            return userContext;


        }
    

        /// <summary>
        /// UNUSED
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="slotName"></param>
        /// <param name="tradeMarketName"></param>
        /// <returns></returns>
        public static UserContext RegisterUser(string sessionId, string slotName, string tradeMarketName) 
        {
            Log.Logger.Information("Creating new UserContext with sessionId: {@sessionId} and slot: {@slotName}",sessionId,slotName);
            UserContext user = new UserContext(sessionId, slotName, TradeMarket.GetTradeMarket(tradeMarketName));
            if (RegisteredUsers.Contains(user))
            {
                Log.Information("UserContext with {@sessionId} : {@slotName} : {@trademarketname} already exists", sessionId, slotName, tradeMarketName);
                return RegisteredUsers.Find(el => el == user);
            }
            RegisteredUsers.Add(user);
            return user;
        }

        
        #endregion
    }
}

