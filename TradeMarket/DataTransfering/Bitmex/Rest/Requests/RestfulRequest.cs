﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.DataTransfering.Bitmex.Rest.Responses;

namespace TradeMarket.DataTransfering.Bitmex.Rest.Requests
{
    public class RestfulRequest<ResultType> : HttpRequestMessage
    {
        public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        public UserAuthentication Authentication;
        public RestfulRequest(string key,string secret,HttpMethod method, string uri,string postdata) : base(method, uri) {
            Authentication = new UserAuthentication(key, secret, method, uri, postdata);
            if(postdata is not null)
            {
                Content = new StringContent(postdata, System.Text.Encoding.UTF8, "application/json");
            }
            Headers.Add("api-key", Authentication.Key);
            Headers.Add("api-signature", Authentication.Signature);
            Headers.Add("api-expires", Authentication.Expires);
        }

    }
}
