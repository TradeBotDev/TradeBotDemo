using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
 

namespace TradeMarket.Services
{
    public class WebSocketApiService
    {
        private ILogger<WebSocketApiService> _logger;

        //private 

        public WebSocketApiService(ILogger<WebSocketApiService> logger)
        {
            _logger = logger;
        }

        public async Task AutharizeUserAsync(string token,string key)
        {

        }

        //тут должны быть конвертеры и все такое. но сегодня я хочу только попробовать достучаться.


    }
}
