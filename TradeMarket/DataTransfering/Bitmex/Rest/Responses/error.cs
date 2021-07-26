using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering.Bitmex.Rest.Responses
{
    public class error
    {

        [DataMember(Name = "message")]
        public string message { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }
    }
}
