using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Responses;

namespace TradeMarket.DataTransfering.Bitmex.Rest.Requests
{
    public class BitmexRestfulRequest : HttpRequestMessage
    {
        public UserAuthentication Authentication;
        public BitmexRestfulRequest(string key,string secret,HttpMethod method, Uri uri,string postdata) : base(method, uri) {
            Authentication = new UserAuthentication(key, secret, method, uri, postdata);
            if(postdata is not null)
            {
                Content = new StringContent(postdata, System.Text.Encoding.UTF8, "applocation/json");
            }
            Headers.Add("api-key", Authentication.Key);
            Headers.Add("api-signature", Authentication.Signature);
            Headers.Add("api-expires", Authentication.Expires);
        }

    }
}
