using Former.Clients;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Former.Models
{
    public class UserContext
    {
        //Класс контекста пользователя
        internal string SessionId => Meta.Sessionid;
        internal string TradeMarket => Meta.Trademarket;
        internal string Slot => Meta.Slot;
        private readonly Storage _storage;
        private readonly TradeMarketClient _tradeMarketClient;
        private readonly Former _former;
        private readonly UpdateHandlers _updateHandlers;
        private readonly HistoryClient _historyClient;
        private bool _isSubscribesAttached;
        private readonly ILogger _logger;

        internal Metadata Meta { get; }

        internal UserContext(string sessionId, string tradeMarket, string slot)
        {
            Meta = new Metadata
            {
                Sessionid = sessionId,
                Trademarket = tradeMarket,
                Slot = slot
            };
            _logger = Log.ForContext("SessionId", Meta.Sessionid)
                         .ForContext("Slot", Meta.Slot)
                         .ForContext("Where", "Former");

            if (!int.TryParse(Environment.GetEnvironmentVariable("RETRY_DELAY"), out var retryDelay)) retryDelay = 10000;

            HistoryClient.Configure(Environment.GetEnvironmentVariable("HISTORY_CONNECTION_STRING"), retryDelay);
            _historyClient = new HistoryClient(_logger);

            TradeMarketClient.Configure(Environment.GetEnvironmentVariable("TRADEMARKET_CONNECTION_STRING"), retryDelay);
            _tradeMarketClient = new TradeMarketClient(_logger);

            _storage = new Storage(_logger);
            _former = new Former(_storage, null, _tradeMarketClient, Meta, _historyClient, Constance.SlotsMultipliers[Meta.Slot], _logger);
            _updateHandlers = new UpdateHandlers(_storage, null, _tradeMarketClient, Meta, _historyClient, Constance.SlotsMultipliers[Meta.Slot], _logger);

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
            _tradeMarketClient.UpdateLotSize += _storage.UpdateLotSize;
            //запускаем подписки на сервис с данными
            _tradeMarketClient.StartObserving(Converters.ConvertMetadata(Meta));
            _isSubscribesAttached = true;
            _logger.Information("Former has been started!");
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
            _tradeMarketClient.UpdateLotSize -= _storage.UpdateLotSize;
            //очищаем хранилище
            _isSubscribesAttached = false;
            _logger.Information("Former has been stopped!");
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
            _storage.BalanceMultiplier = configuration.AvailableBalance;
        }
    }
}
