﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests;

namespace TradeMarket.DataTransfering.Bitmex.Rest.Responses
{
    public class BitmexResfulResponse<MessageType>
    {
        [JsonProperty("error")]
        public ErrorResponse Error { get; set; } = null;

        [JsonIgnore]
        public MessageType Message { get; set; } = default;

        [JsonIgnore]
        private string _responseContent { get; set; }

        public HttpStatusCode Code { get; private set; }

        public BitmexResfulResponse()
        {

        }

        public static BitmexResfulResponse<MessageType> Create(HttpResponseMessage response)
        {
            var result = new BitmexResfulResponse<MessageType>();
            result._ReadContent(response.Content);
            result._TryParse();
            result.Code = response.StatusCode;
            return result;
        }


        private void _TryParse()
        {
            if (IsResponseError())
            {
                Error = JsonConvert.DeserializeObject<BitmexResfulResponse<MessageType>>(_responseContent,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }).Error;
                return;
            }
            //HTML прилетает только если превышен тикрейт
            if (IsResponseHTML())
            {
                Error = new ErrorResponse
                {
                    Message = "Forbiden", Name = "HTML"
                };
                return;
            }
            Message = JsonConvert.DeserializeObject<MessageType>(_responseContent, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

        }

        private bool IsResponseError()
        {
            return _responseContent.Contains("\"error\"");
        }

        private bool IsResponseHTML()
        {
            return _responseContent.Contains("<HTML>");
        }

        private async void _ReadContent(HttpContent content)
        {
            _responseContent = await content.ReadAsStringAsync();
        }
    }
}