using System;
using System.Threading.Tasks;
using Former.Clients;
using Former.Models;
using Grpc.Core;
using Serilog;

namespace Former.Models
{
    public class UserContext
    {
        //Класс контекста пользователя
        internal string SessionId => Meta.GetValue("sessionid");
        internal string TradeMarket => Meta.GetValue("trademarket");
        internal string Slot => Meta.GetValue("slot");
        private readonly Storage _storage;
        private readonly TradeMarketClient _tradeMarketClient;
        private readonly Models.Former _former;
        private readonly UpdateHandlers _updateHandlers;
        private readonly HistoryClient _historyClient;
        private bool _isSubscribesAttached;

        internal Metadata Meta { get; }

        internal UserContext(string sessionId, string tradeMarket, string slot)
        {
            Meta = new Metadata
            {
                { "sessionid", sessionId },
                { "trademarket", tradeMarket },
                { "slot", slot }
            };
            if (!int.TryParse(Environment.GetEnvironmentVariable("RETRY_DELAY"), out var retryDelay)) retryDelay = 10000;
                
            HistoryClient.Configure(Environment.GetEnvironmentVariable("HISTORY_CONNECTION_STRING"), retryDelay);
            _historyClient = new HistoryClient();

            TradeMarketClient.Configure(Environment.GetEnvironmentVariable("TRADEMARKET_CONNECTION_STRING"), retryDelay);
            _tradeMarketClient = new TradeMarketClient();

            _storage = new Storage();
            _former = new Models.Former(_storage, null, _tradeMarketClient, Meta, _historyClient);
            _updateHandlers = new UpdateHandlers(_storage, null, _tradeMarketClient, Meta, _historyClient);
        }

        /// <summary>
        /// Подписывает обработчики из Storage на обновления TradeMarket клиента и запускает подписки непосредственно на сервис с данными
        /// </summary>
        internal void SubscribeStorageToMarket()
        {
            //если мы уже подписаны на обновления - выходим из метода
            if (_isSubscribesAttached) return;
            //подписываем обработчики из Storage на обновления TradeMarket клиента
            _tradeMarketClient.UpdateMarketPrices += _storage.UpdateMarketPrices;
            _tradeMarketClient.UpdateBalance += _storage.UpdateBalance;
            _tradeMarketClient.UpdateMyOrders += _storage.UpdateMyOrderList;
            _tradeMarketClient.UpdatePosition += _storage.UpdatePosition;
            //запускаем подписки на сервис с данными
            _tradeMarketClient.StartObserving(Meta);
            _isSubscribesAttached = true;
            Log.Information("{@Where}: Former has been started!", "Former");
        }

        /// <summary>
        /// Отписывает обработчики из Storage от TradeMarket клиента и отменяет подписки на сервис с данными
        /// </summary>
        internal void UnsubscribeStorage()
        {
            //если мы не подписаны - выходим из метода
            if (!_isSubscribesAttached) return;
            //останавливаем подписку на сервис с данными
            _tradeMarketClient.StopObserving();
            //отписываем обработчики
            _tradeMarketClient.UpdateMarketPrices -= _storage.UpdateMarketPrices;
            _tradeMarketClient.UpdateBalance -= _storage.UpdateBalance;
            _tradeMarketClient.UpdateMyOrders -= _storage.UpdateMyOrderList;
            _tradeMarketClient.UpdatePosition -= _storage.UpdatePosition;
            //очищаем хранилище
            _storage.ClearStorage();
            _isSubscribesAttached = false;
            Log.Information("{@Where}: Former has been stopped!", "Former");
        }

        
        /// <summary>
        /// Вытаскиваем метод формера наружу, чтобы можно было из юзер контекста запросить формирование ордера
        /// </summary>
        internal async Task FormOrder(int decision)
        {
            await _former.FormOrder(decision);
        }

        
        /// <summary>
        /// Вытаскиваем метод формера наружу, чтобы можно было из юзер контекста запросить удаление своих ордеров
        /// </summary>
        internal async Task RemoveAllMyOrders()
        {
            await _former.RemoveAllMyOrders();
        }
        /// <summary>
        /// Выставляем конфигурацию, где это необходимо.
        /// </summary>
        internal void SetConfiguration(Configuration configuration)
        {
            _former.SetConfiguration(configuration);
            _updateHandlers.SetConfiguration(configuration);
        }
    }
}
