using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.ExchangeAccessServiceTests
{
    public class AddExchangeAccessTests : ExchangeAccessServiceTestsData
    {
        [Fact]
        public void AddExchangeToExistingAccountTest()
        {
            string sessionId;

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

            var reply = GenerateLogin("ex_to_acc_").ContinueWith(loginReply => exchangeAccessService.AddExchangeAccess(
                GenerateRequest(loginReply.Result.Result.SessionId), null));

            Assert.Equal(ActionCode.Successful, reply.Result.Result.Result);
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
