using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Algorithm.DataService.v1;

namespace Algorithm.Services
{
    public class DataReceiverService :  DataService.DataServiceBase
    {
        /*public override async Task<AddOrderResponse> AddOrder(AddOrderRequest request, ServerCallContext context)
        {

        }*/
    }
}


////посылается алгоритму
//service DataService
//{
//	/*
//		Перенаправляет стрим закрытых ордеров из ТМа к алгоритму
//	*/
//	rpc AddOrder (stream AddOrderRequest) returns ( AddOrderResponse);

//}

//message AddOrderRequest
//{
//	tradebot.common.v1.Order order = 1;
//}
////Есть ли смысл делать отдельный ответ если ничего не возвращается ?
//message AddOrderResponse
//{
//	google.protobuf.Empty empty = 1;
//}

