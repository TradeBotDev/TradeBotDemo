using Bitmex.Client.Websocket.Responses.Wallets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering.Bitmex.Rest.Requests.Wallets
{
    public class GetWalletRequest : BitmexRestfulRequest<Wallet>
    {
        public GetWalletRequest(string key, string secret, string currency)
            : base(key, secret, HttpMethod.Get, "/api/v1/user/wallet",
                  "{\"currency\":\"" + currency + "\"}")
        {
        }
    }
}
