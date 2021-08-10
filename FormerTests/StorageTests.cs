using System.Threading.Tasks;
using Former.Model;
using Former.Clients;
using Google.Protobuf.WellKnownTypes;
using TradeBot.Common.v1;
using Xunit;

namespace FormerTests
{
    public class StorageTests
    {
        [Fact]
        public async Task UpdateMarketPrices_Bid10AndAskNegative1_Bid10AndAsk0returned()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient)
            {
                BuyMarketPrice = 0,
                SellMarketPrice = 0
            };

            //Act
            await storage.UpdateMarketPrices(10, -1);
            
            //Assert
            Assert.Equal(10,storage.BuyMarketPrice);
            Assert.Equal(0,storage.SellMarketPrice);
        }
        [Fact]
        public async Task UpdateMarketPrices_BidNegative1AndAsk0_Bid0andAsk10returned()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient)
            {
                BuyMarketPrice = 0,
                SellMarketPrice = 0
            };

            //Act
            await storage.UpdateMarketPrices(-1, 10);
            
            //Assert
            Assert.Equal(0,storage.BuyMarketPrice);
            Assert.Equal(10,storage.SellMarketPrice);
        }
        [Fact]
        public async Task UpdateMarketPrices_Bid10AndAsk10_Bid10andAsk10returned()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient)
            {
                BuyMarketPrice = 0,
                SellMarketPrice = 0
            };

            //Act
            await storage.UpdateMarketPrices(10, 10);
            
            //Assert
            Assert.Equal(10,storage.BuyMarketPrice);
            Assert.Equal(10,storage.SellMarketPrice);
        }
        [Fact]
        public async Task UpdateMyOrderList_Order_100QtyBuy_AndPartial_AddedToCounterOrdersWith100Qty()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient)
            {
                CounterOrders = {  }
            };
            var newComingOrder = new Order
            {
                Quantity = 100, Id = "1", Signature = new OrderSignature {Status = OrderStatus.Open, Type = OrderType.Buy}, LastUpdateDate = new Timestamp(), Price = 0
            };
            var changesType = ChangesType.Partitial;

            //Act
            await storage.UpdateMyOrderList(newComingOrder, changesType);

            //Assert
            Assert.Single(storage.CounterOrders);
            Assert.Equal(100, storage.CounterOrders["1"].Quantity);
        }
        [Fact]
        public async Task UpdateMyOrderList_Order_100QtySell_AndPartial_AddedToCounterOrdersWithNegative100Qty()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient)
            {
                CounterOrders = {  }
            };
            var newComingOrder = new Order
            {
                Quantity = 100, Id = "1", Signature = new OrderSignature{ Status = OrderStatus.Open, Type = OrderType.Sell}, LastUpdateDate = new Timestamp(), Price = 0
            };
            var changesType = ChangesType.Partitial;

            //Act
            await storage.UpdateMyOrderList(newComingOrder, changesType);

            //Assert
            Assert.Single(storage.CounterOrders);
            Assert.Equal(-100, storage.CounterOrders["1"].Quantity);
        }
        [Fact]
        public async Task UpdateMyOrderList_Order_200Qty0PrcSell_AndUpdated_Qty200Prc38000InMyOrders()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient);

            var oldOrder = new Order { Quantity = 400, Id = "1", Signature = new OrderSignature { Status = OrderStatus.Open, Type = OrderType.Sell}, LastUpdateDate = new Timestamp(), Price = 38000};

            storage.AddOrder("1", oldOrder, storage.MyOrders);

            var newComingOrder = new Order
            {
                Quantity = 200, Id = "1", Signature = new OrderSignature{ Status = OrderStatus.Open, Type = OrderType.Sell}, LastUpdateDate = new Timestamp(), Price = 0
            };
            var changesType = ChangesType.Update;

            //Act
            await storage.UpdateMyOrderList(newComingOrder, changesType);

            //Assert
            Assert.Single(storage.MyOrders);
            Assert.Empty(storage.CounterOrders);
            Assert.Equal(200, storage.MyOrders["1"].Quantity);
            Assert.Equal(38000, storage.MyOrders["1"].Price);
        }
        [Fact]
        public async Task UpdateMyOrderList_Order_200Qty0PrcBuy_AndUpdated_Qty200Prc38000InCounterOrders()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient);

            var oldOrder = new Order { Quantity = 400, Id = "1", Signature = new OrderSignature { Status = OrderStatus.Open, Type = OrderType.Buy}, LastUpdateDate = new Timestamp(), Price = 38000};

            storage.AddOrder("1", oldOrder, storage.CounterOrders);

            var newComingOrder = new Order
            {
                Quantity = 200, Id = "1", Signature = new OrderSignature{ Status = OrderStatus.Open, Type = OrderType.Buy}, LastUpdateDate = new Timestamp(), Price = 0
            };
            var changesType = ChangesType.Update;

            //Act
            await storage.UpdateMyOrderList(newComingOrder, changesType);

            //Assert
            Assert.Single(storage.CounterOrders);
            Assert.Empty(storage.MyOrders);
            Assert.Equal(200, storage.CounterOrders["1"].Quantity);
            Assert.Equal(38000, storage.CounterOrders["1"].Price);
        }
        [Fact]
        public async Task UpdateMyOrderList_Order_0Qty37000PrcBuy_AndUpdated_Qty400Prc37000InMyOrders()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient);

            var oldOrder = new Order { Quantity = 400, Id = "1", Signature = new OrderSignature { Status = OrderStatus.Open, Type = OrderType.Buy}, LastUpdateDate = new Timestamp(), Price = 38000};

            storage.AddOrder("1", oldOrder, storage.MyOrders);

            var newComingOrder = new Order
            {
                Quantity = 0, Id = "1", Signature = new OrderSignature{ Status = OrderStatus.Open, Type = OrderType.Buy}, LastUpdateDate = new Timestamp(), Price = 37000
            };
            var changesType = ChangesType.Update;

            //Act
            await storage.UpdateMyOrderList(newComingOrder, changesType);

            //Assert
            Assert.Single(storage.MyOrders);
            Assert.Empty(storage.CounterOrders);
            Assert.Equal(400, storage.MyOrders["1"].Quantity);
            Assert.Equal(37000, storage.MyOrders["1"].Price);
        }
        [Fact]
        public async Task UpdateMyOrderList_Order_0Qty37000PrcSell_AndUpdated_Qty400Prc37000InCounterOrders()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient);

            var oldOrder = new Order { Quantity = 400, Id = "1", Signature = new OrderSignature { Status = OrderStatus.Open, Type = OrderType.Sell}, LastUpdateDate = new Timestamp(), Price = 38000};

            storage.AddOrder("1", oldOrder, storage.CounterOrders);

            var newComingOrder = new Order
            {
                Quantity = 0, Id = "1", Signature = new OrderSignature{ Status = OrderStatus.Open, Type = OrderType.Sell}, LastUpdateDate = new Timestamp(), Price = 37000
            };
            var changesType = ChangesType.Update;

            //Act
            await storage.UpdateMyOrderList(newComingOrder, changesType);

            //Assert
            Assert.Single(storage.CounterOrders);
            Assert.Empty(storage.MyOrders);
            Assert.Equal(400, storage.CounterOrders["1"].Quantity);
            Assert.Equal(37000, storage.CounterOrders["1"].Price);
        }
        [Fact]
        public async Task UpdateMyOrderList_Order_0Qty0PrcSell_AndDeleted_MyOrderCount0()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient);

            var oldOrder = new Order { Quantity = 400, Id = "1", Signature = new OrderSignature { Status = OrderStatus.Open, Type = OrderType.Sell}, LastUpdateDate = new Timestamp(), Price = 38000};

            storage.AddOrder("1", oldOrder, storage.CounterOrders);

            var newComingOrder = new Order
            {
                Quantity = 0, Id = "1", Signature = new OrderSignature{ Status = OrderStatus.Closed, Type = OrderType.Sell}, LastUpdateDate = new Timestamp(), Price = 0
            };
            var changesType = ChangesType.Delete;

            //Act
            await storage.UpdateMyOrderList(newComingOrder, changesType);

            //Assert
            Assert.Empty(storage.CounterOrders);
            Assert.Empty(storage.MyOrders);
        }
        [Fact]
        public async Task UpdateMyOrderList_Order_0Qty0PrcBuy_AndDeleted_CounterOrderCount0()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient);

            var oldOrder = new Order { Quantity = 400, Id = "1", Signature = new OrderSignature { Status = OrderStatus.Open, Type = OrderType.Buy}, LastUpdateDate = new Timestamp(), Price = 38000};

            storage.AddOrder("1", oldOrder, storage.CounterOrders);

            var newComingOrder = new Order
            {
                Quantity = 0, Id = "1", Signature = new OrderSignature{ Status = OrderStatus.Closed, Type = OrderType.Buy}, LastUpdateDate = new Timestamp(), Price = 0
            };
            var changesType = ChangesType.Delete;

            //Act
            await storage.UpdateMyOrderList(newComingOrder, changesType);

            //Assert
            Assert.Empty(storage.CounterOrders);
            Assert.Empty(storage.MyOrders);
        }
        [Fact]
        public void UpdatePosition_QtyNegative400_QtyNegative400returned()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient)
            {
                PositionSize = 0
            };

            //Act
            storage.UpdatePosition(-400);

            //Assert
            Assert.Equal(-400,storage.PositionSize);
        }
        [Fact]
        public void UpdatePosition_Qty400_400returned()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient)
            {
                PositionSize = 0
            };

            //Act
            storage.UpdatePosition(400);

            //Assert
            Assert.Equal(400,storage.PositionSize);
        }
        [Fact]
        public void UpdateBalance_Available0AndTotal400_Available400AndTotal400returned()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient)
            {
                AvailableBalance = 400,
                TotalBalance = 100
            };

            //Act
            storage.UpdateBalance(0,400);

            //Assert
            Assert.Equal(400,storage.AvailableBalance);
            Assert.Equal(400,storage.TotalBalance);
        }
        [Fact]
        public void UpdateBalance_Available400AndTotal0_Available400AndTotal400returned()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient)
            {
                AvailableBalance = 100,
                TotalBalance = 400
            };

            //Act
            storage.UpdateBalance(400,0);

            //Assert
            Assert.Equal(400,storage.AvailableBalance);
            Assert.Equal(400,storage.TotalBalance);
        }
        [Fact]
        public void UpdateBalance_AvailableNegative400AndTotalNegative400_Available100AndTotal100returned()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient)
            {
                AvailableBalance = 100,
                TotalBalance = 100
            };

            //Act
            storage.UpdateBalance(-400,-400);

            //Assert
            Assert.Equal(100,storage.AvailableBalance);
            Assert.Equal(100,storage.TotalBalance);
        }
        [Fact]
        public void RemoveOrder_Id1_MyOrdersCount0()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient);
            storage.MyOrders.TryAdd("1",new Order{ Id = "1", Price = 0, Quantity =0, Signature = new OrderSignature(), LastUpdateDate = new Timestamp()});

            //Act
            var actual = storage.RemoveOrder("1", storage.MyOrders);

            //Assert
            Assert.Empty(storage.MyOrders);
            Assert.True(actual);
        }
        [Fact]
        public void RemoveOrder_Id1_CounterOrdersCount0()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient);
            storage.CounterOrders.TryAdd("1",new Order{ Id = "1", Price = 0, Quantity =0, Signature = new OrderSignature(), LastUpdateDate = new Timestamp()});

            //Act
            var actual = storage.RemoveOrder("1", storage.CounterOrders);

            //Assert
            Assert.Empty(storage.CounterOrders);
            Assert.True(actual);
        }
        [Fact]
        public void UpdateOrder_Qty0Prc37000_Qty100Prc37000InMyOrders()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient);
            storage.MyOrders.TryAdd("1",new Order{ Id = "1", Price = 38000, Quantity = 100, Signature = new OrderSignature(), LastUpdateDate = new Timestamp()});
            var newComingOrder = new Order
            {
                Quantity = 0, Id = "1", Signature = new OrderSignature(), LastUpdateDate = new Timestamp(), Price = 37000
            };
            //Act
            var actual = storage.UpdateOrder(newComingOrder, storage.MyOrders);

            //Assert
            Assert.Equal(100,storage.MyOrders["1"].Quantity);
            Assert.Equal(37000,storage.MyOrders["1"].Price);
            Assert.Empty(storage.CounterOrders);
            Assert.Single(storage.MyOrders);
            Assert.True(actual);
        }
        [Fact]
        public void UpdateOrder_Qty0Prc37000_Qty100Prc37000InCounterOrders()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient);
            storage.CounterOrders.TryAdd("1",new Order{ Id = "1", Price = 38000, Quantity = 100, Signature = new OrderSignature(), LastUpdateDate = new Timestamp()});
            var newComingOrder = new Order
            {
                Quantity = 0, Id = "1", Signature = new OrderSignature(), LastUpdateDate = new Timestamp(), Price = 37000
            };
            //Act
            var actual = storage.UpdateOrder(newComingOrder, storage.CounterOrders);

            //Assert
            Assert.Equal(100,storage.CounterOrders["1"].Quantity);
            Assert.Equal(37000,storage.CounterOrders["1"].Price);
            Assert.Empty(storage.MyOrders);
            Assert.Single(storage.CounterOrders);
            Assert.True(actual);
        }
        [Fact]
        public void UpdateOrder_Qty400Prc0_Qty400Prc38000InCounterOrders()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient);
            storage.CounterOrders.TryAdd("1",new Order{ Id = "1", Price = 38000, Quantity = 100, Signature = new OrderSignature(), LastUpdateDate = new Timestamp()});
            var newComingOrder = new Order
            {
                Quantity = 400, Id = "1", Signature = new OrderSignature(), LastUpdateDate = new Timestamp(), Price = 0
            };
            //Act
            var actual = storage.UpdateOrder(newComingOrder, storage.CounterOrders);

            //Assert
            Assert.Equal(400,storage.CounterOrders["1"].Quantity);
            Assert.Equal(38000,storage.CounterOrders["1"].Price);
            Assert.Empty(storage.MyOrders);
            Assert.Single(storage.CounterOrders);
            Assert.True(actual);
        }
        [Fact]
        public void UpdateOrder_Qty400Prc0_Qty400Prc38000InMyOrders()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient);
            storage.MyOrders.TryAdd("1",new Order{ Id = "1", Price = 38000, Quantity = 100, Signature = new OrderSignature(), LastUpdateDate = new Timestamp()});
            var newComingOrder = new Order
            {
                Quantity = 400, Id = "1", Signature = new OrderSignature(), LastUpdateDate = new Timestamp(), Price = 0
            };
            //Act
            var actual = storage.UpdateOrder(newComingOrder, storage.MyOrders);

            //Assert
            Assert.Equal(400,storage.MyOrders["1"].Quantity);
            Assert.Equal(38000,storage.MyOrders["1"].Price);
            Assert.Empty(storage.CounterOrders);
            Assert.Single(storage.MyOrders);
            Assert.True(actual);
        }
        [Fact]
        public void UpdateOrder_Id2_updateCounterOrderReturnedFalse()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient);
            storage.CounterOrders.TryAdd("1",new Order{ Id = "1", Price = 0, Quantity = 0, Signature = new OrderSignature(), LastUpdateDate = new Timestamp()});

            var newComingOrder = new Order
            {
                Quantity = 0, Id = "2", Signature = new OrderSignature(), LastUpdateDate = new Timestamp(), Price = 0
            };
            //Act
            var actual1 = storage.UpdateOrder(newComingOrder, storage.CounterOrders);

            //Assert
            Assert.False(actual1);
        }
        [Fact]
        public void UpdateOrder_Id2_updateMyOrderReturnedFalse()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient);
            storage.MyOrders.TryAdd("1",new Order{ Id = "1", Price = 0, Quantity = 0, Signature = new OrderSignature(), LastUpdateDate = new Timestamp()});

            var newComingOrder = new Order
            {
                Quantity = 0, Id = "2", Signature = new OrderSignature(), LastUpdateDate = new Timestamp(), Price = 0
            };
            //Act
            var actual = storage.UpdateOrder(newComingOrder, storage.MyOrders);

            //Assert
            Assert.False(actual);
        }
        [Fact]
        public void AddOrder_Order_addToMyOrdersReturnedTrue()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient);
            
            var newComingOrder = new Order
            {
                Quantity = 0, Id = "1", Signature = new OrderSignature(), LastUpdateDate = new Timestamp(), Price = 0
            };
            //Act
            var actual = storage.AddOrder(newComingOrder.Id,newComingOrder, storage.MyOrders);

            //Assert
            Assert.True(actual);
        }
        [Fact]
        public void AddOrder_Order_addToCounterOrdersOrdersReturnedTrue()
        {
            //Arrange
            var HistoryClient = new HistoryClient();
            var storage = new Storage(HistoryClient);
            
            var newComingOrder = new Order
            {
                Quantity = 0, Id = "1", Signature = new OrderSignature(), LastUpdateDate = new Timestamp(), Price = 0
            };
            //Act
            var actual = storage.AddOrder(newComingOrder.Id,newComingOrder, storage.CounterOrders);

            //Assert
            Assert.True(actual);
        }
    }
}
