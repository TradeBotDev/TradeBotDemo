using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.ExchangeAccessServiceTests
{
    [Collection("AccountTests")]
    public class ExchangeBySessionTests : ExchangeAccessServiceTestsData
    {
        [Fact]
        public void GetNotExistingExchangeAccessTest()
        {
            // Пока пусто
        }
        
        // Тестирование получения информации о бирже из несуществующего аккаунта.
        [Fact]
        public void GetExchangeAccessFromNonExistingAccount()
        {
            // Очистка списка вошедших аккаунтов для того, чтобы не было конфликтов.
            AccountGRPC.Models.State.loggedIn = new();

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
