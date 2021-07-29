using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountGRPC.Models;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.ExchangeAccessServiceTests
{
    [Collection("AccountTests")]
    public class ExchangeBySessionTests : ExchangeAccessServiceTestsData
    {
        // Тестирование получения несуществующей информации о доступе к бирже из существующего.
        [Fact]
        public void GetNotExistingExchangeAccessTest()
        {
            // Очистка списка вошедших аккаунтов для того, чтобы не было конфликтов.
            State.loggedIn = new();

            // Последовательная регистрация, вход и получение информации о доступе к бирже.
            var reply = GenerateLogin("not_existing_exchange").ContinueWith(loginReply => 
                exchangeAccessService.ExchangeBySession(new ExchangeBySessionRequest
                {
                    SessionId = loginReply.Result.Result.SessionId,
                    Code = ExchangeCode.Bitmex
                }, null));

            // Ожидается что в результате не будет найдена информация о бирже.
            Assert.Equal(ActionCode.ExchangeNotFound, reply.Result.Result.Result);
        }
        
        // Тестирование получения информации о доступе к бирже из несуществующего аккаунта.
        [Fact]
        public void GetExchangeAccessFromNonExistingAccount()
        {
            // Очистка списка вошедших аккаунтов для того, чтобы не было конфликтов.
            State.loggedIn = new();

            // Запрос с заведомо несуществующим аккаунтом.
            var request = new ExchangeBySessionRequest
            {
                Code = ExchangeCode.Bitmex,
                SessionId = "non_existing_session_id"
            };
            var reply = exchangeAccessService.ExchangeBySession(request, null);

            // Ожидается, что в результате аккаунт не будет найден.
            Assert.Equal(ActionCode.AccountNotFound, reply.Result.Result);
        }

        [Fact]
        public void GetExistingExchangeAccess()
        {
            // Пока пусто
        }
    }
}
