using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Former
{
    public class Sender
    {
        public static async void SendShopingList(List<string> formedShoppingList)
        {
            //var sendOrderClient = new FormerService.FormerServiceClient(TradeMarketChannel);
            //using var call = sendOrderClient.BuyOrder();

            //var readTask = Task.Run(async () =>
            //{
            //    await foreach (var response in call.ResponseStream.ReadAllAsync())
            //    {
            //        if (response.Reply.Code != 0) Console.WriteLine(response.Reply.Message);
            //    }
            //});
            //foreach (var id in formedShoppingList)
            //{
            //    await call.RequestStream.WriteAsync(new BuyOrderRequest() { Id = id });
            //}
            //await call.RequestStream.CompleteAsync();
            //await readTask;
        }

    }
}
