using System.Threading.Tasks;
using Former.Clients;
using Former.Model;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using TradeBot.Common.v1;
using Xunit;

namespace FormerTests
{
    public class FormerTests
    {
        [Fact]
        public void FormOrder_AvailBal240000AndBuyAndPosition100AndPrc260000_PlaceOrderNotCalled()
        {
            //Arrange
            var historyClient = new HistoryClient();
            var storage = new Storage
            {
                BuyMarketPrice = 38000,
                AvailableBalance = 240000,
                TotalBalance = 1200000,
                PositionSize = 100
            };

            var config = new Config
            {
                AvaibleBalance = 1.0,
                RequiredProfit = 0.01,
                ContractValue = 100,
                OrderUpdatePriceRange = 1
            };

            TradeMarketClient.Configure("http://localhost:5005", 10000);
            var tradeMarketClient = new TradeMarketClient();
            var former = new Former.Model.Former(storage, config, tradeMarketClient, Metadata.Empty, historyClient);

            //Act
            var exception = Record.ExceptionAsync(async () => await former.FormOrder(1));

            //Assert
            Assert.Equal(TaskStatus.RanToCompletion, exception.Status);
        }
        [Fact]
        public void FormOrder_AvailBal1200000AndBuyAndPosition100AndPrc260000_PlaceOrderCalled()
        {
            //Arrange
            var historyClient = new HistoryClient();
            var storage = new Storage
            {
                BuyMarketPrice = 38000,
                AvailableBalance = 1200000,
                TotalBalance = 1200000,
                PositionSize = 100
            };

            var config = new Config
            {
                AvaibleBalance = 1.0,
                RequiredProfit = 0.01,
                ContractValue = 100,
                OrderUpdatePriceRange = 1
            };

            TradeMarketClient.Configure("http://localhost:5005", 10000);
            var tradeMarketClient = new TradeMarketClient();
            var former = new Former.Model.Former(storage, config, tradeMarketClient, Metadata.Empty, historyClient);

            //Act
            var exception = Record.ExceptionAsync(async () => await former.FormOrder(1));

            //Assert
            Assert.Equal(TaskStatus.WaitingForActivation, exception.Status);
        }
        [Fact]
        public void FormOrder_AvailBal240000AndSellAndPositionNegative100AndPrc260000_PlaceOrderNotCalled()
        {
            //Arrange
            var historyClient = new HistoryClient();
            var storage = new Storage
            {
                SellMarketPrice = 38000,
                AvailableBalance = 240000,
                TotalBalance = 1200000,
                PositionSize = -100
            };

            var config = new Config
            {
                AvaibleBalance = 1.0,
                ContractValue = 100
            };

            TradeMarketClient.Configure("http://localhost:5005", 10000);
            var tradeMarketClient = new TradeMarketClient();
            var former = new Former.Model.Former(storage, config, tradeMarketClient, Metadata.Empty, historyClient);

            //Act
            var exception = Record.ExceptionAsync(async () => await former.FormOrder(-1));

            //Assert
            Assert.Equal(TaskStatus.RanToCompletion, exception.Status);
        }
        [Fact]
        public void FormOrder_AvailBal1200000AndSellAndPositionNegative100AndPrc260000_PlaceOrderCalled()
        {
            //Arrange
            var historyClient = new HistoryClient();
            var storage = new Storage
            {
                SellMarketPrice = 38000,
                AvailableBalance = 1200000,
                TotalBalance = 1200000,
                PositionSize = -100
            };

            var config = new Config
            {
                AvaibleBalance = 1.0,
                RequiredProfit = 0.01,
                ContractValue = 100
            };

            TradeMarketClient.Configure("http://localhost:5005", 10000);
            var tradeMarketClient = new TradeMarketClient();
            var former = new Former.Model.Former(storage, config, tradeMarketClient, Metadata.Empty, historyClient);

            //Act
            var exception = Record.ExceptionAsync(async () => await former.FormOrder(-1));

            //Assert
            Assert.Equal(TaskStatus.WaitingForActivation, exception.Status);
        }
        [Fact]
        public void FormOrder_TotalBal1200000AndBuyAndPositionNegative100AndMyOrdersMargin1050000AndPrc260000_PlaceOrderNotCalled()
        {
            //Arrange
            var historyClient = new HistoryClient();
            var storage = new Storage
            {
                BuyMarketPrice = 38000,
                AvailableBalance = 1200000,
                TotalBalance = 1200000,
                PositionSize = -100
            };
            var order = new Order
            {
                Quantity = 400, Id = "",
                Signature = new OrderSignature { Status = OrderStatus.Open, Type = OrderType.Buy },
                LastUpdateDate = new Timestamp(), Price = 38000
            };
            storage.MyOrders.TryAdd(order.Id,order);

            var config = new Config
            {
                AvaibleBalance = 1.0,
                ContractValue = 100
            };

            TradeMarketClient.Configure("http://localhost:5005", 10000);
            var tradeMarketClient = new TradeMarketClient();
            var former = new Former.Model.Former(storage, config, tradeMarketClient, Metadata.Empty, historyClient);

            //Act
            var exception = Record.ExceptionAsync(async () => await former.FormOrder(1));

            //Assert
            Assert.Equal(TaskStatus.RanToCompletion, exception.Status);
        }
        [Fact]
        public void FormOrder_TotalBal1200000AndBuyAndPositionNegative100AndCountersOrdersMargin1050000AndPrc260000_PlaceOrderNotCalled()
        {
            //Arrange
            var historyClient = new HistoryClient();
            var storage = new Storage
            {
                BuyMarketPrice = 38000,
                AvailableBalance = 1200000,
                TotalBalance = 1200000,
                PositionSize = -100
            };
            var order = new Order
            {
                Quantity = 400, Id = "",
                Signature = new OrderSignature { Status = OrderStatus.Open, Type = OrderType.Buy },
                LastUpdateDate = new Timestamp(), Price = 38000
            };
            storage.CounterOrders.TryAdd(order.Id,order);

            var config = new Config
            {
                AvaibleBalance = 1.0,
                ContractValue = 100
            };

            TradeMarketClient.Configure("http://localhost:5005", 10000);
            var tradeMarketClient = new TradeMarketClient();
            var former = new Former.Model.Former(storage, config, tradeMarketClient, Metadata.Empty, historyClient);

            //Act
            var exception = Record.ExceptionAsync(async () => await former.FormOrder(1));

            //Assert
            Assert.Equal(TaskStatus.RanToCompletion, exception.Status);
        }
        [Fact]
        public void FormOrder_TotalBal1200000AndBuyAndPositionNegative100AndCountersMyOrdersMargin520000AndPrc260000_PlaceOrderCalled()
        {
            //Arrange
            var historyClient = new HistoryClient();
            var storage = new Storage
            {
                BuyMarketPrice = 38000,
                AvailableBalance = 1200000,
                TotalBalance = 1200000,
                PositionSize = -100
            };
            var order = new Order
            {
                Quantity = 100, Id = "",
                Signature = new OrderSignature { Status = OrderStatus.Open, Type = OrderType.Buy },
                LastUpdateDate = new Timestamp(), Price = 38000
            };
            storage.CounterOrders.TryAdd(order.Id,order);
            storage.MyOrders.TryAdd(order.Id,order);
            var config = new Config
            {
                AvaibleBalance = 1.0,
                ContractValue = 100
            };

            TradeMarketClient.Configure("http://localhost:5005", 10000);
            var tradeMarketClient = new TradeMarketClient();
            var former = new Former.Model.Former(storage, config, tradeMarketClient, Metadata.Empty, historyClient);

            //Act
            var exception = Record.ExceptionAsync(async () => await former.FormOrder(1));

            //Assert
            Assert.Equal(TaskStatus.WaitingForActivation, exception.Status);
        }
        [Fact]
        public void FormOrder_TotalBal1200000AndBuyAndPositionNegative100AndPrc1315789_PlaceOrderNotCalled()
        {
            //Arrange
            var historyClient = new HistoryClient();
            var storage = new Storage
            {
                BuyMarketPrice = 38000,
                AvailableBalance = 1200000,
                TotalBalance = 1200000,
                PositionSize = -100
            };
            var config = new Config
            {
                AvaibleBalance = 1.0,
                ContractValue = 500
            };

            TradeMarketClient.Configure("http://localhost:5005", 10000);
            var tradeMarketClient = new TradeMarketClient();
            var former = new Former.Model.Former(storage, config, tradeMarketClient, Metadata.Empty, historyClient);

            //Act
            var exception = Record.ExceptionAsync(async () => await former.FormOrder(1));

            //Assert
            Assert.Equal(TaskStatus.RanToCompletion, exception.Status);
        }
        [Fact]
        public void FormOrder_TotalBal1200000AndSellAndPosition100AndMyOrdersMargin1050000AndPrc260000_PlaceOrderNotCalled()
        {
            //Arrange
            var historyClient = new HistoryClient();
            var storage = new Storage
            {
                SellMarketPrice = 38000,
                AvailableBalance = 1200000,
                TotalBalance = 1200000,
                PositionSize = 100
            };
            var order = new Order
            {
                Quantity = 400, Id = "",
                Signature = new OrderSignature { Status = OrderStatus.Open, Type = OrderType.Buy },
                LastUpdateDate = new Timestamp(), Price = 38000
            };
            storage.MyOrders.TryAdd(order.Id,order);

            var config = new Config
            {
                AvaibleBalance = 1.0,
                ContractValue = 100
            };

            TradeMarketClient.Configure("http://localhost:5005", 10000);
            var tradeMarketClient = new TradeMarketClient();
            var former = new Former.Model.Former(storage, config, tradeMarketClient, Metadata.Empty, historyClient);

            //Act
            var exception = Record.ExceptionAsync(async () => await former.FormOrder(-1));

            //Assert
            Assert.Equal(TaskStatus.RanToCompletion, exception.Status);
        }
        [Fact]
        public void FormOrder_TotalBal1200000AndSellAndPosition100AndCountersOrdersMargin1050000AndPrc260000_PlaceOrderNotCalled()
        {
            //Arrange
            var historyClient = new HistoryClient();
            var storage = new Storage
            {
                SellMarketPrice = 38000,
                AvailableBalance = 1200000,
                TotalBalance = 1200000,
                PositionSize = 100
            };
            var order = new Order
            {
                Quantity = 400, Id = "",
                Signature = new OrderSignature { Status = OrderStatus.Open, Type = OrderType.Buy },
                LastUpdateDate = new Timestamp(), Price = 38000
            };
            storage.CounterOrders.TryAdd(order.Id,order);

            var config = new Config
            {
                AvaibleBalance = 1.0,
                ContractValue = 100
            };

            TradeMarketClient.Configure("http://localhost:5005", 10000);
            var tradeMarketClient = new TradeMarketClient();
            var former = new Former.Model.Former(storage, config, tradeMarketClient, Metadata.Empty, historyClient);

            //Act
            var exception = Record.ExceptionAsync(async () => await former.FormOrder(-1));

            //Assert
            Assert.Equal(TaskStatus.RanToCompletion, exception.Status);
        }
        [Fact]
        public void FormOrder_TotalBal1200000AndSellAndPosition100AndCountersMyOrdersMargin520000AndPrc260000_PlaceOrderCalled()
        {
            //Arrange
            var historyClient = new HistoryClient();
            var storage = new Storage
            {
                SellMarketPrice = 38000,
                AvailableBalance = 1200000,
                TotalBalance = 1200000,
                PositionSize = 100
            };
            var order = new Order
            {
                Quantity = 100, Id = "",
                Signature = new OrderSignature { Status = OrderStatus.Open, Type = OrderType.Buy },
                LastUpdateDate = new Timestamp(), Price = 38000
            };
            storage.CounterOrders.TryAdd(order.Id,order);
            storage.MyOrders.TryAdd(order.Id,order);
            var config = new Config
            {
                AvaibleBalance = 1.0,
                ContractValue = 100
            };

            TradeMarketClient.Configure("http://localhost:5005", 10000);
            var tradeMarketClient = new TradeMarketClient();
            var former = new Former.Model.Former(storage, config, tradeMarketClient, Metadata.Empty, historyClient);

            //Act
            var exception = Record.ExceptionAsync(async () => await former.FormOrder(-1));

            //Assert
            Assert.Equal(TaskStatus.WaitingForActivation, exception.Status);
        }
        [Fact]
        public void FormOrder_TotalBal1200000AndSellAndPosition100AndPrc1315789_PlaceOrderNotCalled()
        {
            //Arrange
            var historyClient = new HistoryClient();
            var storage = new Storage
            {
                SellMarketPrice = 38000,
                AvailableBalance = 1200000,
                TotalBalance = 1200000,
                PositionSize = 100
            };
            var config = new Config
            {
                AvaibleBalance = 1.0,
                ContractValue = 500
            };

            TradeMarketClient.Configure("http://localhost:5005", 10000);
            var tradeMarketClient = new TradeMarketClient();
            var former = new Former.Model.Former(storage, config, tradeMarketClient, Metadata.Empty, historyClient);

            //Act
            var exception = Record.ExceptionAsync(async () => await former.FormOrder(-1));

            //Assert
            Assert.Equal(TaskStatus.RanToCompletion, exception.Status);
        }
    }
}
