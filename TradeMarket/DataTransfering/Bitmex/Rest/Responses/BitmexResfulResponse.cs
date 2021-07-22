using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests;

namespace TradeMarket.DataTransfering.Bitmex.Rest.Responses
{
    public class BitmexResfulResponse<TResultType> : HttpResponseMessage
    {


        public BitmexResfulResponse(BitmexRestfulRequest request) : base()
        {
            
        }
    }
}
