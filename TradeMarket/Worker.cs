﻿using Bitmex.Client.Websocket.Responses.Orders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Clients;
using TradeMarket.DataTransfering;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests.Ammend;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests.Place;

namespace TradeMarket
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly AccountClient _account;

        public Worker(ILogger<Worker> logger, AccountClient account)
        {
            _account = account;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                AccountClient._accountClient = _account;
                var request = new PlaceOrderRequest("key", "secret",new Order() { 
                    OrdType = "Market", OrderQty = 200, Side = Bitmex.Client.Websocket.Responses.BitmexSide.Sell
                });
                //запуск подписок
                Task[] tasks =
                {
                    //BitmexPublisher.GetInstance().SubscribeAsync(stoppingToken),
                   /* FakeOrderPublisher.GetInstance().Simulate(),
                    FakeBalancePublisher.GetInstance().Simulate(),*/
                    FakeSlotPublisher.GetInstance().Simulate()
                };
                await Task.WhenAll(tasks);
            }
        }
    }
}