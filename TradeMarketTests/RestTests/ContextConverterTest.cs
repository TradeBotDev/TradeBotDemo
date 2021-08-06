using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Responses.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests;
using Xunit;
using Xunit.Extensions;

namespace TradeMarketTests.RestTests
{
    public class ContextConverterTest
    {
        public static IEnumerable<object[]> data
        {
            get =>
                new List<object[]>{
                    new object[]{ new Order{ Account = 41931231, Side = BitmexSide.Buy, Currency = "XBTUSD" } },
                    new object[]{ new AmmendOrderDTO { Id = "asdsad", LeavesQuantity = null, Price = 5123, Quantity = null } }
                };
        }

        [Theory]
        [MemberData(nameof(data))]
        public void IsConvertedProperly(object obj)
        {
            string json = ContextConverter.Convert(obj);
            //json должен быть в одну строку
            Assert.DoesNotContain("\n", json);
            //json не должен содержать null полей
            Assert.DoesNotContain("null", json);
            //не должно быть выражений вида BitmexSide.Sell
            Assert.False(Regex.Match(json, "\\.[a-zA-Z]*").Success);
        }
    }
}
