using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Algorithm.Services
{
    public class OrderWrapper
    {
		public double quantity;
		public double price;
		public OrderType orderType;
		public OrderStatus orderStatus;
		public DateTime LastUpdateDate;
		public string id;


    }
	public enum OrderType
	{
		ORDER_TYPE_UNSPECIFIED,
		ORDER_TYPE_SELL,
		ORDER_TYPE_BUY
	}
	public enum OrderStatus
	{
		ORDER_STATUS_UNSPECIFIED,
		ORDER_STATUS_OPEN,
		ORDER_STATUS_CLOSED
	}
}