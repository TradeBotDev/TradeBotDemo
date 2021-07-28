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
    public class AddExchangeAccessTests : ExchangeAccessServiceTestsData
    {
        [Fact]
        public void AddExchangeToExistingAccountTest()
        {
            State.loggedIn = new();

            string sessionId = "Отсутствует";

            AddExchangeAccessRequest GenerateRequest(string _sessionId)
            {
                sessionId = _sessionId;
                return new AddExchangeAccessRequest
                {
                    Code = ExchangeCode.Bitmex,
                    ExchangeName = "Bitmex",
                    Secret = "test_secret",
                    Token = "test_token",
                    SessionId = _sessionId
                };
            }

            using (var database = new AccountContext())
            {
                var reply = GenerateLogin("ex_to_acc")
                .ContinueWith(loginReply => exchangeAccessService.AddExchangeAccess(GenerateRequest(loginReply.Result.Result.SessionId), null));

                var accounts = database.Accounts.Where(account => account.Email == "ex_to_acc_generated_user@pochta.ru");
                var exchanges = database.ExchangeAccesses.Where(exchange => exchange.Account.AccountId == accounts.First().AccountId);
                foreach (AccountGRPC.Models.ExchangeAccess exchange in exchanges)
                    database.Remove(exchange);
                database.SaveChanges();

                Assert.Equal(ActionCode.Successful, reply.Result.Result.Result);
            }
        }

        [Fact]
        public void AddExchangeToNonExistsAccountTest()
        {
            var request = new AddExchangeAccessRequest
            {
                Code = ExchangeCode.Bitmex,
                ExchangeName = "Bitmex",
                Secret = "test_secret",
                Token = "test_token",
                SessionId = "this_id_is_not_exists"
            };

            var reply = exchangeAccessService.AddExchangeAccess(request, null);
            Assert.Equal(ActionCode.AccountNotFound, reply.Result.Result);
        }
    }
}
