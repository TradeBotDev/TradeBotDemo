﻿using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Websockets;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;
using TradeMarket.Clients;
using TradeMarket.DataTransfering;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using Margin = Bitmex.Client.Websocket.Responses.Margins.Margin;
using Order = Bitmex.Client.Websocket.Responses.Orders.Order;

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
        public event EventHandler<Order> UserOrders;
        public event EventHandler<Model.Balance> UserBalance;
        public event EventHandler<IPublisher<Margin>.ChangedEventArgs> UserMargin;
        public event EventHandler<IPublisher<Position>.ChangedEventArgs> UserPosition;


        public List<IPublisher<Margin>.ChangedEventArgs> MarginCache = new List<IPublisher<Margin>.ChangedEventArgs>();
        public List<Balance> BalanceCache = new List<Balance>();

        //TODO сделать эти классы абстрактными
        internal BitmexWebsocketClient WSClient { get; set; }
        internal BitmexRestfulClient RestClient { get; set; }

        private AccountClient _accountClient;

        /// <summary>
        /// Метод инициализации контекста. 
        /// </summary>
        public void init()
        {

            var keySecretPair = _accountClient.GetUserInfo(SessionId);
            Key = keySecretPair.Key;
            Secret = keySecretPair.Secret;

            //TODO что-то сделать с этим методом
            AutheticateUser();

            //инициализация подписок
            TradeMarket.SubscribeToBalance((sender, el) => { BalanceCache.Add(el); UserBalance?.Invoke(sender, el); }, this);
            TradeMarket.SubscribeToBook((sender, el) => {Book?.Invoke(sender, el); }, this);
            TradeMarket.SubscribeToBook25((sender, el) => Book25?.Invoke(sender, el), this);
            TradeMarket.SubscribeToUserOrders((sender, el) => UserOrders?.Invoke(sender, el), this);
            TradeMarket.SubscribeToUserMargin((sender, el) => UserMargin?.Invoke(sender, el), this);
            TradeMarket.SubscribeToUserPositions((sender, el) => UserPosition?.Invoke(sender, el), this);
        }

        /// <summary>
        /// После создание нового объекта необходима инициализация некоторых полей и ивентов через метод init()
        /// </summary>
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
            lock (locker)
            {
                Log.Logger.Information("Getting UserContext {@sessionId} : {@slotName} : {@tradeMarketName}", sessionId, slotName, tradeMarketName);
                Log.Logger.Information("Stored Contexts : {@RegisteredUsers}", RegisteredUsers);
                userContext = RegisteredUsers.FirstOrDefault(el => el.IsEquevalentTo(sessionId, slotName, tradeMarketName));
                if (userContext is null)
                {
                    Log.Logger.Information("Creating new UserContext {@sessionId} : {@slotName} : {@tradeMarketName}", sessionId, slotName, tradeMarketName);
                    userContext = new UserContext(sessionId, slotName, TradeMarket.GetTradeMarket(tradeMarketName));
                    //контекст сначала добавляется , а затеми инициализируется для того чтобы избежать создание нескольких контекстов
                    RegisteredUsers.Add(userContext);
                    userContext.init();
                }
                return userContext;
            }

        }
        #endregion
    }
}

