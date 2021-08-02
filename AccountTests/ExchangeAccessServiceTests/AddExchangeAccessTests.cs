using AccountGRPC.Models;
using System.Linq;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.ExchangeAccessServiceTests
{
    [Collection("AccountTests")]
    public class AddExchangeAccessTests : ExchangeAccessServiceTestsData
    {
        // Тестирование добавления информации о доступе к бирже в существующий аккаунт.
        [Fact]
        public void AddExchangeToExistingAccountTest()
        {
            string sessionId = "none";

            // Локальный метод, который сгенерирует запрос с Id сессии, полученным при входе, а также
            // запишет Id сессии в переменную.
            AddExchangeAccessRequest GenerateRequest(string _sessionId)
            {
                sessionId = _sessionId;
                return new AddExchangeAccessRequest
                {
                    SessionId = _sessionId,
                    Code = ExchangeAccessCode.Bitmex,
                    ExchangeName = "Bitmex",
                    Token = "test_token",
                    RefreshToken = "test_refresh_token",
                    LogoutToken = "test_logout_token",
                    Secret = "test_secret"
                };
            }

            using (var database = new AccountContext())
            {
                // Генерация нового пользователя и вход в него.
                var reply = GenerateLogin("ex_to_acc")
                    .ContinueWith(loginReply => exchangeAccessService.AddExchangeAccess(
                        GenerateRequest(loginReply.Result.Result.SessionId), null));

                // Ожидается, что добавление новой информации о доступе к бирже будет успешно завершено.
                Assert.Equal(ExchangeAccessActionCode.Successful, reply.Result.Result.Result);

                // Очистка данных.
                var accounts = database.Accounts.Where(account => account.Email == "ex_to_acc_generated_user@pochta.ru").First();
                var exchanges = database.ExchangeAccesses.Where(exchange => exchange.Account.AccountId == accounts.AccountId);
                foreach (AccountGRPC.Models.ExchangeAccess exchange in exchanges)
                    database.Remove(exchange);
                database.SaveChanges();
            }
        }

        // Тестирование попытки добавления информации о доступе к бирже в несуществующий аккаунт.
        [Fact]
        public void AddExchangeToNonExistsAccountTest()
        {
            // Запрос с заведомо несуществующим Id сессии.
            var request = new AddExchangeAccessRequest
            {
                Code = ExchangeAccessCode.Bitmex,
                ExchangeName = "Bitmex",
                Secret = "test_secret",
                Token = "test_token",
                SessionId = "this_id_is_not_exists"
            };
            var reply = exchangeAccessService.AddExchangeAccess(request, null);

            // Ожидается, что появится сообщение об ошибке, что аккаунт не был найденен.
            Assert.Equal(ExchangeAccessActionCode.AccountNotFound, reply.Result.Result);
        }
    }
}
