using AccountGRPC.Models;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.ExchangeAccessServiceTests
{
    [Collection("AccountTests")]
    public class AllExchangesBySessionTests : ExchangeAccessServiceTestsData
    {
        // Тестирование получения информации о всех добавленных биржах в аккаунт, когда они точно есть.
        [Fact]
        public void WhenExchangesIsExistsTest()
        {
            State.loggedIn = new();

            // Переменная, в которую будет записываться итоговый Id сессии.
            string sessionId = "none";

            // Локальный метод, который необходим для того, чтобы записать Id сессии при использовании объекта
            // запроса в качестве параметра.
            AddExchangeAccessRequest GenerateRequest(string _sessionId)
            {
                sessionId = _sessionId;
                return new AddExchangeAccessRequest
                {
                    Code = ExchangeCode.Bitmex,
                    ExchangeName = "Bitmex",
                    SessionId = _sessionId,
                    Token = "test_token_12345",
                    Secret = "test_secret_12345"
                };
            }

            // Последовательная регистрация и вход (внутри метода GenerateLogin), добавление информации о
            // доступе к бирже, а затем ее чтение.
            var reply = GenerateLogin("all_exs_from_acc")
                .ContinueWith(loginReply => exchangeAccessService.AddExchangeAccess(GenerateRequest(loginReply.Result.Result.SessionId), null))
                .ContinueWith(addExchangeReply => exchangeAccessService.AllExchangesBySession(
                new SessionRequest { SessionId = sessionId }, null));

            // Ожидается, что придет сообщение об успешности получения данных, и коллекция с биржами
            // не будет пустой.
            Assert.Equal(ActionCode.Successful, reply.Result.Result.Result);
            Assert.NotEmpty(reply.Result.Result.Exchanges);
        }

        // Тестирование получения информации о биржах, которой в действительности нет.
        [Fact]
        public void WhenExchangesIsNotExistsTest()
        {
            State.loggedIn = new();

            var reply = GenerateLogin("ex_not_exist")
                .ContinueWith(loginReply => exchangeAccessService.AllExchangesBySession(
                    new SessionRequest { SessionId = loginReply.Result.Result.SessionId }, null));

            Assert.Equal(ActionCode.ExchangeNotFound, reply.Result.Result.Result);
            Assert.Empty(reply.Result.Result.Exchanges);
        }

        [Fact]
        public void WhenAccountIsNotExistsTest()
        {
            var reply = exchangeAccessService.AllExchangesBySession(
                new SessionRequest { SessionId = "not_existing_session" }, null);

            Assert.Equal(ActionCode.AccountNotFound, reply.Result.Result);
            Assert.Empty(reply.Result.Exchanges);
        }
    }
}
