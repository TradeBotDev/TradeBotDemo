using System;

namespace Former.Models
{
    public class Configuration
    {
        public double AvailableBalance;
        public double RequiredProfit;
        public double ContractValue;
        public double OrderUpdatePriceRange;
        public AlgorithmInfo AlgorithmInfo;
    }

    public class Metadata
    {
        public string Sessionid;
        public string Trademarket;
        public string Slot;
        public string UserId;
    }

    public class AlgorithmInfo
    {
        public TimeSpan Interval;
        public int Sensitivity;
    }

    public class Order
    {
        public string Id;
        public double Quantity;
        public double Price;
        public DateTimeOffset LastUpdateDate;
        public OrderSignature Signature;
    }

    public enum OrderStatus
    {
        ORDER_STATUS_UNSPECIFIED,
        ORDER_STATUS_OPEN,
        ORDER_STATUS_CLOSED
    }

    public enum OrderType
    {
        ORDER_TYPE_UNSPECIFIED,
        ORDER_TYPE_SELL,
        ORDER_TYPE_BUY
    }

    public class OrderSignature
    {
        public OrderStatus Status;
        public OrderType Type;
    }


    public enum ChangesType
    {
        CHANGES_TYPE_UNDEFIEND,
        CHANGES_TYPE_PARTITIAL,
        CHANGES_TYPE_UPDATE,
        CHANGES_TYPE_INSERT,
        CHANGES_TYPE_DELETE,
    }

    public enum ReplyCode
    {
        REPLY_CODE_UNSPECIFIED,
        REPLY_CODE_SUCCEED,
        REPLY_CODE_FAILURE
    }

    public class DefaultResponse
    {
        public ReplyCode Code;
        public string Message;
    }
    public class Balance
    {
        public string Currency;
        public string Value;
    }
}
