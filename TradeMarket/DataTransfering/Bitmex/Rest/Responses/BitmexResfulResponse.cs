using Newtonsoft.Json;
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

        public async static Task<BitmexResfulResponse<MessageType>> Create(HttpResponseMessage response)
        {
            var result = new BitmexResfulResponse<MessageType>();
            await result._ReadContent(response.Content);
            result._TryParse();
            result.Code = response.StatusCode;
            return result;
        }


        private void _TryParse()
        {
            var serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            if (IsResponseError())
            {
                Error = JsonConvert.DeserializeObject<BitmexResfulResponse<MessageType>>(_responseContent,serializerSettings).Error;
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
            Message = JsonConvert.DeserializeObject<MessageType>(_responseContent,serializerSettings);

        }

        private bool IsResponseError()
        {
            return ContainsProperty(_responseContent,"error");
        }

        /// <summary>
        /// Проверяет содержится ли <paramref name="value"/> в <paramref name="response"/>. Если <paramref name="value"/> пусто или null то оно содержится 
        /// </summary>
        private static bool ContainsValue(string response,string value)
        {
            return string.IsNullOrEmpty(value) || response.Contains($"\"{value}\"");
        }

        /// <summary>
        /// Проверяет содержится ли <paramref name="property"/> в <paramref name="response"/>. Если <paramref name="value"/> пусто или null то оно содержится 
        private static bool ContainsProperty(string response,string property)
        {
            return string.IsNullOrEmpty(property) || response.Contains($"\"{property}\":");
        }

        private bool IsResponseHTML()
        {
            return ContainsValue(_responseContent,"<HTML>");
        }

        private async Task _ReadContent(HttpContent content)
        {
            _responseContent = await content.ReadAsStringAsync();
        }
    }
}
