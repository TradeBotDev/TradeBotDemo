using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Instruments;
using Bitmex.Client.Websocket.Responses.Margins;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Wallets;
using Serilog;
using System.Threading;
using TradeMarket.DataTransfering.Bitmex.Rest.Responses;
using TradeMarket.Model.Publishers;

namespace TradeMarket.Model.UserContexts
{
    public class Context :  ICloneable, IEquatable<Context>
    {
        public ContextSignature Signature { get; set; }

        public string Key { get; set; } = null;

        public string Secret { get; set; } = null;

        public TradeMarkets.TradeMarket TradeMarket { get; set; }

        public TaskCompletionSource<Context> AutharizationCompleted { get; private set; } = new TaskCompletionSource<Context>();

        public Context(Context other) :this()
        {
            Signature.SessionId = other.Signature.SessionId;
            Signature.SlotName = other.Signature.SlotName;
            Signature.TradeMarketName = other.Signature.TradeMarketName;
            Key = other.Key;
            Secret = other.Secret;
        }

        public Context() : this(new ContextSignature())
        {

        }
        public Context(ContextSignature signature)
        {
            Signature = signature;
        }

        public async Task<List<Position>> SubscribeToUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.SubscribeToAsync(TradeMarket.PositionPublisher, handler, this, TradeMarket.PublisherFactory.CreateUserPositionPublisher, token, logger);

        }
        public async Task<List<Margin>> SubscribeToUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.SubscribeToAsync(TradeMarket.MarginPublisher, handler, this, TradeMarket.PublisherFactory.CreateUserMarginPublisher, token, logger);

        }
        public async Task<List<Order>> SubscribeToUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.SubscribeToAsync(TradeMarket.OrderPublisher, handler, this, TradeMarket.PublisherFactory.CreateUserOrderPublisher, token, logger);

        }
        public async Task<List<Wallet>> SubscribeToBalance(EventHandler<IPublisher<Wallet>.ChangedEventArgs> handler, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.SubscribeToAsync(TradeMarket.WalletPublishers, handler, this, TradeMarket.PublisherFactory.CreateWalletPublisher, token, logger);
        }
        public async Task<List<BookLevel>> SubscribeToBook25UpdatesAsync(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.SubscribeToAsync(TradeMarket.Book25Publisher, handler, this, TradeMarket.PublisherFactory.CreateBook25Publisher, token, logger);
        }
        public async Task<List<Instrument>> SubscribeToInstrumentUpdate(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.SubscribeToInstrumentsAsync(handler, this, token, logger);
        }

        public async Task UnSubscribeFromBook25UpdatesAsync(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler, ILogger logger)
        {
            await TradeMarket.UnsubscribeFromAsync(TradeMarket.Book25Publisher, this, handler, logger);
        }
        public async Task UnSubscribeFromInstrumentUpdate(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler, ILogger logger)
        {
            await TradeMarket.UnsubscribeFromAsync(TradeMarket.InstrumentPublisher, this, handler, logger);
        }
        public async Task UnSubscribeFromUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler, ILogger logger)
        {
            await TradeMarket.UnsubscribeFromAsync(TradeMarket.PositionPublisher, this, handler, logger);
        }
        public async Task UnSubscribeFromUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler, ILogger logger)
        {
            await TradeMarket.UnsubscribeFromAsync(TradeMarket.MarginPublisher, this, handler, logger);
        }
        public async Task UnSubscribeFromUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler, ILogger logger)
        {
            await TradeMarket.UnsubscribeFromAsync(TradeMarket.OrderPublisher, this, handler, logger);
        }
        public async Task UnSubscribeFromBalance(EventHandler<IPublisher<Wallet>.ChangedEventArgs> handler, ILogger logger)
        {
            await TradeMarket.UnsubscribeFromAsync(TradeMarket.WalletPublishers, this, handler, logger);
        }

        public async Task<ResfulResponse<Order>> PlaceOrder(double quontity, double price, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.PlaceOrderAsync(quontity, price, this, token, logger);
        }

        public async Task<ResfulResponse<Order[]>> DeleteOrder(string id, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.DeleteOrderAsync(id, this, token, logger);
        }

        public async Task<ResfulResponse<Order>> AmmendOrder(string id, double? price, long? Quantity, long? LeavesQuantity, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.AmmendOrderAsync(id, price, Quantity, LeavesQuantity, this, token, logger);
        }

        public async Task<bool> AutheticateUser(CancellationToken token, ILogger logger)
        {
            bool result = await TradeMarket.AutheticateUserAsync(this, token, logger);
            if (result == true)
            {
                AutharizationCompleted.SetResult(this);
            }
            else
            {
                AutharizationCompleted.SetException(new ArgumentException("Provided Key and Secret is not valid"));
            }
            return result;
        }

        public bool IsEquevalentTo(string sessionId, string slotName, string tradeMarketName)
        {
            bool sessionCheck = Signature.SessionId == sessionId || sessionId == null;
            bool slotNameCheck = Signature.SlotName == slotName || slotName == null;
            bool tradeMarketNameCheck = Signature.TradeMarketName == tradeMarketName || tradeMarketName == null;
            return sessionCheck && slotNameCheck &&  tradeMarketNameCheck;
        }

        public static bool operator == (Context left,Context right)
        {
            return left.Signature == right.Signature;
        }

        public static bool operator !=(Context left, Context right)
        {
            return left.Signature != right.Signature;
        }

        public override bool Equals(object obj)
        {
            return obj is Context && this.Equals(obj as Context);
                   
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Signature);
        }

        public bool Equals(Context other)
        {
            return EqualityComparer<ContextSignature>.Default.Equals(Signature, other.Signature);
        }

        public object Clone()
        {
            return new Context(this);
        }
    }
}
