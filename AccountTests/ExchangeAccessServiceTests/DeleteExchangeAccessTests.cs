using AccountGRPC.Models;
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
            // Очистка списка вошедших аккаунтов для того, чтобы не было конфликтов.
            State.loggedIn = new();
            string sessionId = "none";

            // Локальный метод, генерирующий запрос на добавление информации о доступе к бирже
            // и записывающий Id сессии в переменную.
            AddExchangeAccessRequest GenerateRequest(string _sessionId)
            {
                sessionId = _sessionId;
                return new AddExchangeAccessRequest
                {
                    Code = ExchangeCode.Bitmex,
                    ExchangeName = "Bitmex",
                    SessionId = sessionId,
                    Token = "test_token",
                    Secret = "test_secret"
                };
            }

            // Последовательная генерация входа в аккаунт, добавление информации о доступе к биржи в аккаунт,
            // а затем ее удаление.
            var reply = GenerateLogin("delete_existing_exchange")
                .ContinueWith(loginReply => exchangeAccessService.AddExchangeAccess(GenerateRequest(loginReply.Result.Result.SessionId), null))
                .ContinueWith(none => exchangeAccessService.DeleteExchangeAccess(new DeleteExchangeAccessRequest
                {
                    SessionId = sessionId,
                    Code = ExchangeCode.Bitmex
                }, null));

            // Ожидается, что удаление информации будет завершено успешно.
            Assert.Equal(ActionCode.Successful, reply.Result.Result.Result);
        }
    }
}
