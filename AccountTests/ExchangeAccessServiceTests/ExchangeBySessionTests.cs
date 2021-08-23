using AccountGRPC.Models;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.ExchangeAccessServiceTests
{
    [Collection("AccountTests")]
    public class ExchangeBySessionTests : ExchangeAccessServiceTestsData
    {
        // Тестирование получения несуществующей информации о доступе к бирже из существующего аккаунта.
        [Fact]
        public void GetNotExistingExchangeAccessTest()
        {
            // Последовательная регистрация, вход и получение информации о доступе к бирже.
            var reply = GenerateLogin("getting_not_existing_exchange").ContinueWith(loginReply => 
                exchangeAccessService.ExchangeBySession(new ExchangeBySessionRequest
                {
                    SessionId = loginReply.Result.SessionId,
                    Code = ExchangeAccessCode.Bitmex
                }, null));

            // Ожидается что в результате не будет найдена информация о бирже.
            Assert.Equal(ExchangeAccessActionCode.IsNotFound, reply.Result.Result.Result);
        }
        
        // Тестирование получения информации о доступе к бирже из несуществующего аккаунта.
        [Fact]
        public void GetExchangeAccessFromNonExistingAccountTest()
        {
            // Запрос с заведомо несуществующим аккаунтом.
            var request = new ExchangeBySessionRequest
            {
                Code = ExchangeAccessCode.Bitmex,
                SessionId = "non_existing_session_id"
            };
            var reply = exchangeAccessService.ExchangeBySession(request, null);

            // Ожидается, что в результате аккаунт не будет найден.
            Assert.Equal(ExchangeAccessActionCode.AccountNotFound, reply.Result.Result);
        }

        // Тестирование получения существующей информации о доступе к бирже из существующего аккаунта.
        [Fact]
        public void GetExistingExchangeAccessTest()
        {
            string sessionId = "none";

            // Локальный метод, генерирующий запрос на добавление информации о доступе к бирже
            // и записывающий Id сессии в переменную.
            AddExchangeAccessRequest GenerateRequest(string _sessionId)
            {
                sessionId = _sessionId;
                return new AddExchangeAccessRequest
                {
                    Code = ExchangeAccessCode.Bitmex,
                    ExchangeName = "Bitmex",
                    SessionId = sessionId,
                    Token = "test_token",
                    Secret = "test_secret"
                };
            }

            // Последовательная генерация входа в аккаунт, добавление информации о доступе к биржи в аккаунт,
            // а затем ее чтение.
            var reply = GenerateLogin("one_existing_exchange")
                .ContinueWith(loginReply => exchangeAccessService.AddExchangeAccess(GenerateRequest(loginReply.Result.SessionId), null))
                .ContinueWith(none => exchangeAccessService.ExchangeBySession(new ExchangeBySessionRequest
                {
                    SessionId = sessionId,
                    Code = ExchangeAccessCode.Bitmex
                }, null));

            // Ожидается, что получение информации будет завершено успешно и объект с информацией не будет пустым.
            Assert.Equal(ExchangeAccessActionCode.Successful, reply.Result.Result.Result);
            Assert.NotNull(reply.Result.Result.Exchange);
        }
    }
}
