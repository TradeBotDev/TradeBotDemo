using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.AccountServiceTests
{
    public class LoginTests : AccountServicesTestsData
    {
        // Тестирование входа в существующий аккаунт.
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

        // Тестирование входа в несуществующий аккаунт.
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

        // Тестирование входа в уже вошедший аккаунт.
        [Fact]
        public void DoubleLogin()
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
                .ContinueWith(login => service.Login(loginRequest, null));

            Assert.Equal(ActionCode.Successful, reply.Result.Result.Result);
        }
    }
}
