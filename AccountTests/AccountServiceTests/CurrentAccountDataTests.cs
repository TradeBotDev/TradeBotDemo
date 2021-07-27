using AccountGRPC.Models;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.AccountServiceTests
{
    public class CurrentAccountDataTests : AccountServiceTestsData
    {
        // Тестирование получения данных из существующего аккаунта.
        [Fact]
        public void DataFromExistingAccount()
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
            .ContinueWith(login => service.CurrentAccountData(new SessionRequest
            {
                SessionId = login.Result.Result.SessionId
            }, null));

            Assert.Equal(ActionCode.Successful, reply.Result.Result.Result);
        }

        // Тестирование получения данных из несуществующего аккаунта.
        [Fact]
        public void DataFromNonExistingAccount()
        {
            var request = new SessionRequest { SessionId = "non_existing_session_id" };
            State.loggedIn = new();
            var reply = service.CurrentAccountData(request, null);
            Assert.Equal(ActionCode.AccountNotFound, reply.Result.Result);
        }
    }
}
