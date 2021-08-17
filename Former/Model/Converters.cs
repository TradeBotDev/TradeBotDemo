using Google.Protobuf.WellKnownTypes;
using TradeBot.Common.v1;
namespace Former.Model
{
    public class Converters
    {
        public static Order ConvertOrder(TradeBot.Common.v1.Order order)
        {
            var status = order.Signature.Status == TradeBot.Common.v1.OrderStatus.Open
                ? OrderStatus.ORDER_STATUS_OPEN
                : OrderStatus.ORDER_STATUS_CLOSED;
            var type = order.Signature.Type == TradeBot.Common.v1.OrderType.Sell ? OrderType.ORDER_TYPE_SELL : OrderType.ORDER_TYPE_BUY;

            return new Order
            {
                Id = order.Id,
                LastUpdateDate = order.LastUpdateDate.ToDateTimeOffset(),
                Price = order.Price,
                Quantity = order.Quantity,
                Signature = new OrderSignature
                {
                    Status = status,
                    Type = type
                }
            };
        }

        public static TradeBot.Common.v1.Order ConvertOrder(Order order)
        {
            var status = order.Signature.Status == OrderStatus.ORDER_STATUS_OPEN
                ? TradeBot.Common.v1.OrderStatus.Open
                : TradeBot.Common.v1.OrderStatus.Closed;
            var type = order.Signature.Type == OrderType.ORDER_TYPE_SELL ? TradeBot.Common.v1.OrderType.Sell : TradeBot.Common.v1.OrderType.Buy;

            return new TradeBot.Common.v1.Order
            {
                Id = order.Id,
                LastUpdateDate = new Timestamp{Seconds = order.LastUpdateDate.ToUnixTimeSeconds()},
                Price = order.Price,
                Quantity = order.Quantity,
                Signature = new TradeBot.Common.v1.OrderSignature
                {
                    Status = status,
                    Type = type
                }
            };
        }

        public static Configuration ConvertConfiguration(Config order)
        {
            return new Configuration
            {
                AlgorithmInfo = new AlgorithmInfo
                {
                    Interval = order.AlgorithmInfo.Interval.ToTimeSpan(),
                    Sensitivity = order.AlgorithmInfo.Sensitivity
                },
                AvailableBalance = order.AvaibleBalance,
                ContractValue = order.ContractValue,
                OrderUpdatePriceRange = order.OrderUpdatePriceRange,
                RequiredProfit = order.RequiredProfit
            };
        }

        public static Config ConvertConfiguration(Configuration order)
        {
            return new Config
            {
                AlgorithmInfo = new TradeBot.Common.v1.AlgorithmInfo
                {
                    Interval = order.AlgorithmInfo.Interval.ToDuration(),
                    Sensitivity = order.AlgorithmInfo.Sensitivity
                },
                AvaibleBalance = order.AvailableBalance,
                ContractValue = order.ContractValue,
                OrderUpdatePriceRange = order.OrderUpdatePriceRange,
                RequiredProfit = order.RequiredProfit
            };
        }

        public static DefaultResponse ConvertDefaultResponse(TradeBot.Common.v1.DefaultResponse defaultResponse)
        {
            return new DefaultResponse
            {
                Code = (ReplyCode)defaultResponse.Code,
                Message = defaultResponse.Message
            };
        }

        public static TradeBot.Common.v1.DefaultResponse ConvertDefaultResponse(DefaultResponse defaultResponse)
        {
            return new TradeBot.Common.v1.DefaultResponse
            {
                Code = (TradeBot.Common.v1.ReplyCode)defaultResponse.Code,
                Message = defaultResponse.Message
            };
        }

        public static Balance ConvertBalance(TradeBot.Common.v1.Balance balance)
        {
            return new Balance
            {
                Currency = balance.Currency,
                Value = balance.Value
            };
        }

        public static TradeBot.Common.v1.Balance ConvertBalance(Balance balance)
        {
            return new TradeBot.Common.v1.Balance
            {
                Currency = balance.Currency,
                Value = balance.Value
            };
        }
    }
}
