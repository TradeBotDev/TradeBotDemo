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
    interface IPublic<T>
    {
        GrpcChannel channel { get; }
        T client { get; }
    }
    interface IEmpty
    {

    }
    public class TradeMarketClientClass:IPublic<TMClient>,IEmpty
    {
        //static GrpcChannel trademarketChannel = GrpcChannel.ForAddress("https://localhost:5005");
        //public TMClient clientTradeMarket = new TMClient(trademarketChannel);

        public GrpcChannel channel => GrpcChannel.ForAddress("https://localhost:5005");
        public TMClient client => new TMClient(channel);

        public T SubBal<T>(T req)
        {
            //пустышка
            T newT=req;
            return newT;
        }
    }
    class RelayClientClass : IPublic<RelayClient>,IEmpty
    {
        public GrpcChannel channel => GrpcChannel.ForAddress("https://localhost:5004");
        public RelayClient client => new RelayClient(channel);

        public T Switch<T>(T req)
        {
            //пустышка
            T newT = req;
            return newT;
        }
    }

    public class Repeater
    {
        GrpcChannel trademarketChannel = GrpcChannel.ForAddress("https://localhost:5005");
        GrpcChannel relayChannel = GrpcChannel.ForAddress("https://localhost:5004");
        GrpcChannel autorizationChannel = GrpcChannel.ForAddress("https://localhost:5000");

        TMClient clientTradeMarket;
        RelayClient clientRelay;
        //еще для аутентификации


        IEmpty empty1 = new TradeMarketClientClass();
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
        
        public void RedirectToTheServer()
        {
            
        }

        //public static T1 ReturnToTheClient<T,T1>(this T1 response, T response, string methodName)
        //{

        //}
    }
}
