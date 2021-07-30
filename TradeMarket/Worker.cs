using Bitmex.Client.Websocket.Responses.Orders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using StackExchange.Redis;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Clients;
using TradeMarket.DataTransfering;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests.Ammend;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests.Place;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests.Wallets;
using TradeMarket.Model;

namespace TradeMarket
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConnectionMultiplexer _multiplexer;
        private readonly AccountClient _account;

        public Worker(ILogger<Worker> logger, AccountClient account,IConnectionMultiplexer multiplexer)
        {
            _multiplexer = multiplexer;
            _account = account;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            AccountClient._accountClient = _account;
            await _multiplexer.GetSubscriber().SubscribeAsync("Bitmex_Book25", (channel, value) => { Log.Information("{@value}", value.ToString()); });

            while (!stoppingToken.IsCancellationRequested)
            {
                
               
            }
        }
    }
}