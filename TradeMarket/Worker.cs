using Bitmex.Client.Websocket.Responses.Orders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using StackExchange.Redis;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Clients;
using TradeMarket.DataTransfering;
using TradeMarket.DataTransfering.Bitmex.Model;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests.Ammend;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests.Place;
using TradeMarket.DataTransfering.Bitmex.Rest.Requests.Wallets;
using TradeMarket.Model;
using TradeMarket.Model.TradeMarkets;
using TradeMarket.Model.UserContexts;

namespace TradeMarket
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConnectionMultiplexer _multiplexer;
        private readonly TradeMarketFactory _factory;
        private readonly AccountClient _account;

        public Worker(ILogger<Worker> logger, AccountClient account,TradeMarketFactory factory)
        {
            _factory = factory;
            _account = account;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            AccountClient._accountClient = _account;
            _factory.SubscribeToLifeLineTopics(_factory.GetTradeMarket("bitmex") as BitmexTradeMarket, stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                
               
            }
        }
    }
}