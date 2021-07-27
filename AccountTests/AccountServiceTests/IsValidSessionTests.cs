using AccountGRPC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.AccountServiceTests
{
    public class IsValidSessionTests : AccountServicesTestsData
    {
        [Fact]
        public void ExistingAccountIsValidTest()
        {
            var registerRequest = new RegisterRequest
            {
                Email = $"existing_user{random.Next(0, 10000)}@pochta.test",
                Password = "password",
                VerifyPassword = "password"
            };

            var loginRequest = new LoginRequest()
            {
                Email = registerRequest.Email,
                Password = "password",
                SaveExchangesAfterLogout = false
            };

            var reply = service.Register(registerRequest, null)
            .ContinueWith(login => service.Login(loginRequest, null))
            .ContinueWith(login => service.IsValidSession(new SessionRequest
            {
                SessionId = login.Result.Result.SessionId 
            }, null));

            Assert.True(reply.Result.Result.IsValid);
        }

        [Fact]
        public void NonExistingAccountIsValidTest()
        {
            var request = new SessionRequest { SessionId = "non_existing_session_id" };
            State.loggedIn = new();
            var reply = service.IsValidSession(request, null);
            Assert.False(reply.Result.IsValid);
        }
    }
}
