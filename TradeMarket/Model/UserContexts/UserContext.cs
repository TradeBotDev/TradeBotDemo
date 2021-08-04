using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Instruments;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Wallets;
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
using TradeMarket.Model.Publishers;
using Margin = Bitmex.Client.Websocket.Responses.Margins.Margin;
using Order = Bitmex.Client.Websocket.Responses.Orders.Order;

namespace TradeMarket.Model.UserContexts
{
    public class UserContext : IEquatable<UserContext>
    {
        #region Dynamic Part
        public String SessionId { get; internal set; }
        public String SlotName { get; internal set; }

        public String Key { get; internal set; }
        public String Secret { get; internal set; }

        public Model.TradeMarkets.TradeMarket TradeMarket { get; set; }

        public event EventHandler<IPublisher<BookLevel>.ChangedEventArgs> Book25;
        public event EventHandler<IPublisher<BookLevel>.ChangedEventArgs> Book;
        public event EventHandler<IPublisher<Order>.ChangedEventArgs> UserOrders;
        public event EventHandler<IPublisher<Wallet>.ChangedEventArgs> UserBalance;
        public event EventHandler<IPublisher<Margin>.ChangedEventArgs> UserMargin;
        public event EventHandler<IPublisher<Position>.ChangedEventArgs> UserPosition;
        public event EventHandler<IPublisher<Instrument>.ChangedEventArgs> InstrumentUpdate;



        //TODO тут должен быть кэш из редиса

        //Клиенты для доступа к личной информации пользователя на бирже
        internal BitmexWebsocketClient WSClient { get; set; }
        internal BitmexRestfulClient RestClient { get; set; }


        public void AssignKeySecret(string key,string secret)
        {
            Key = key;
            Secret = secret;
        }

        /// <summary>
        /// Метод инициализации контекста. 
        /// </summary>
        public void init()
        {

            AutheticateUser();

            //инициализация подписок
            TradeMarket.SubscribeToBalance((sender, el) => {UserBalance?.Invoke(sender, el); }, this);
            //TradeMarket.SubscribeToBook((sender, el) => {Book?.Invoke(sender, el); }, this);
            TradeMarket.SubscribeToBook25((sender, el) => Book25?.Invoke(sender, el), this);
            TradeMarket.SubscribeToUserOrders((sender, el) => UserOrders?.Invoke(sender, el), this);
            TradeMarket.SubscribeToUserMargin((sender, el) => UserMargin?.Invoke(sender, el), this);
            TradeMarket.SubscribeToUserPositions((sender, el) => UserPosition?.Invoke(sender, el), this);
            TradeMarket.SubscribeToInstruments((sender, el) => InstrumentUpdate?.Invoke(sender, el), this);
        }

        public UserContext()
        {

        }

        /// <summary>
        /// После создание нового объекта необходима инициализация некоторых полей и ивентов через метод init()
        /// </summary>
        internal UserContext(string sessionId, string slotName, Model.TradeMarkets.TradeMarket tradeMarket)
        {

            SessionId = sessionId;
            SlotName = slotName;

            TradeMarket = tradeMarket;
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
            return await TradeMarket.AutheticateUser(this);
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

        
    }
}

