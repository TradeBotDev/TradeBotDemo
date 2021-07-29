using AccountGRPC.Models;
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
    public class DeleteExchangeAccessTests : ExchangeAccessServiceTestsData
    {
        // Тестирование удаления несуществующей информации о доступе к бирже из существующего аккаунта.
        [Fact]
        public void DeleteNotExistingExchangeAccessTest()
        {
            // Очистка списка вошедших аккаунтов для того, чтобы не было конфликтов.
            State.loggedIn = new();

            // Последовательная регистрация, вход и удаление информации о доступе к бирже.
            var reply = GenerateLogin("not_existing_exchange_deletion").ContinueWith(loginReply =>
                exchangeAccessService.DeleteExchangeAccess(new DeleteExchangeAccessRequest
                {
                    SessionId = loginReply.Result.Result.SessionId,
                    Code = ExchangeCode.Bitmex
                }, null));

            // Ожидается что в результате не будет найдена информация о бирже.
            Assert.Equal(ActionCode.ExchangeNotFound, reply.Result.Result.Result);
        }

        // Тестирование на удаление информации о доступе к бирже из несуществующего аккаунта.
        [Fact]
        public void DeleteExchangeAccessFromNonExistingAccountTest()
        {
            // Очистка списка вошедших аккаунтов для того, чтобы не было конфликтов.
            State.loggedIn = new();

            // Запрос с заведомо несуществующим аккаунтом.
            var request = new DeleteExchangeAccessRequest
            {
                Code = ExchangeCode.Bitmex,
                SessionId = "non_existing_session_id"
            };
            var reply = exchangeAccessService.DeleteExchangeAccess(request, null);

            // Ожидается, что в результате аккаунт не будет найден.
            Assert.Equal(ActionCode.AccountNotFound, reply.Result.Result);
        }

        [Fact]
        public void GetExistingExchangeAccessTest()
        {
            // Пока пусто
        }
    }
}
