using System.Collections.Generic;

using TradeBot.TradeMarket.TradeMarketService.v1;

namespace Former
{
    public class ReplyComparator : IComparer<SubscribeOrdersResponse>
    {
        int IComparer<SubscribeOrdersResponse>.Compare(SubscribeOrdersResponse x, SubscribeOrdersResponse y)
        {
            return x.Response.Order.Price.CompareTo(y .Response.Order.Price);
        }
    }
}
