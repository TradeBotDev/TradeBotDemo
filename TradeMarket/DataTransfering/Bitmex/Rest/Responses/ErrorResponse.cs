using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering.Bitmex.Rest.Responses
{
    public class ErrorResponse
    {

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
