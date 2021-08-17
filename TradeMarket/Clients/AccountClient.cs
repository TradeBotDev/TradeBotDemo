using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
namespace TradeMarket.Clients
{
    public class AccountClient
    {
        #region Singleton

        /// <summary>
        /// Используется для метода  <code> GetInstanse()</code>
        /// Не следует использовать это поле для доступа к синглтон объекту
        /// Не следует изменять значение этого поля
        /// </summary>
        internal static AccountClient _accountClient = null;

        /// <summary>
        /// Объект <code>AccountClient</code>
        /// создается с помощью DI через Startup.cs 
        /// и конфигурируется через Worker.cs
        /// этот метод следует использовать для доступа к объекту
        /// </summary>
        /// <returns></returns>
        public static AccountClient GetInstance()
        {
            return _accountClient;
        }

        #endregion


        #region Client
        private readonly ExchangeAccess.ExchangeAccessClient _client;

        public AccountClient(ExchangeAccess.ExchangeAccessClient client)
        {
            _client = client;
        }

        public async Task<UserAccessInfo> GetUserInfoAsync(string sessionId)
        {
            Log.Logger.Information($"Fetching key and secret by sessionId {sessionId}");

            var reply =  await _client.ExchangeBySessionAsync(new()
            {
                Code = ExchangeAccessCode.Bitmex,
                SessionId = sessionId
            });
            Log.Logger.Information($"Fetching complete with result : {reply.Result}");
            if(reply.Result != ExchangeAccessActionCode.Successful)
            {
                //Если по переданному sessionId нет данных
                throw new KeyNotFoundException($"sessionId : {sessionId} has no data in AccountService");
            }
            var key = reply.Exchange.Token;
            var secret = reply.Exchange.Secret;
            return new UserAccessInfo(key, secret);
        }
        #endregion
    }
    /// <summary>
    /// DTO для информации о ключе и секретке пользователя на бирже
    /// </summary>
    public record UserAccessInfo(String Key, String Secret);
}


