using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.AccountServiceTests
{
    public class LoginTests
    {
        Random random = new Random();
        AccountGRPC.AccountService service = new AccountGRPC.AccountService();

        [Fact]
        public void LoginToExistingAccount()
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
            var reply = service.Register(registerRequest, null);
            reply.ContinueWith(login => service.Login(loginRequest, null));

            Assert.Equal(ActionCode.Successful, reply.Result.Result);
        }

        [Fact]
        public void LoginToNonExistingAccount()
        {
            var request = new LoginRequest()
            {
                Email = $"non_existing_user{random.Next(0, 10000)}@pochta.ru",
                Password = "password",
                SaveExchangesAfterLogout = false
            };

            var reply = service.Login(request, null);
            Assert.Equal(ActionCode.AccountNotFound, reply.Result.Result);
        }
    }
}
