using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Responses.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests.Place;
using Xunit;

namespace TradeMarketTests.RestTests
{
    public class HttpHeadersTest
    {

        /// <summary>
        /// /проверка того что при отправке запроса произайдет авторизация на бирже (при правильном ключе)
        /// </summary>
        [Theory]
        [InlineData("0n8sicC9Y8v3iuwtDDkJ44IO", "PhVLNBRGA199lGgrQ2bbf59Ux7yRsgwkn-sfigW7rMOPoPWh")]
        public async void IsQueryAutheticateCorrectlyAsync(string key, string secret)         
        {
            var client = new RestfulClient(BitmexRestufllLink.Testnet);
            var request = new PlaceOrderRequest(key, secret, new Order());

            var response = await client.SendAsync(request, new System.Threading.CancellationToken());
            Assert.NotEqual(System.Net.HttpStatusCode.Unauthorized, response.Code);
        }



    }
}
