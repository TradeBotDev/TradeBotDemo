using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Facade.FacadeService.v1;
using TMClient = TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService.TradeMarketServiceClient;
using RelayClient = TradeBot.Relay.RelayService.v1.RelayService.RelayServiceClient;
using TradeBot.TradeMarket.TradeMarketService.v1;
using static TradeBot.TradeMarket.TradeMarketService.v1.TradeMarketService;

namespace Facade
{
    interface IClient
    {
        
    }
    public class MyClass: IClient
    {

    }

    public class Repeater: TradeMarketServiceBase
    {
        GrpcChannel trademarketChannel = GrpcChannel.ForAddress("https://localhost:5005");
        GrpcChannel relayChannel = GrpcChannel.ForAddress("https://localhost:5004");
        GrpcChannel autorizationChannel = GrpcChannel.ForAddress("https://localhost:5000");

        TMClient clientTradeMarket;
        RelayClient clientRelay;
        //еще для аутентификации
        


        private static Repeater _repeater;
        private Repeater()
        {
            clientTradeMarket =new TMClient(trademarketChannel);
            clientRelay = new RelayClient(relayChannel);
            //client
        }

        public static Repeater GetInstance()
        {
            return _repeater ??= new Repeater();
        }
        
        public T1 RedirectToTheServer<T, T1>(T request)
        {
            T1 answer;

            return;
        }

        //public static T1 ReturnToTheClient<T,T1>(this T1 response, T response, string methodName)
        //{

        //}
    }
}
