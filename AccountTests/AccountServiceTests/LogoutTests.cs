using AccountGRPC.Models;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.AccountServiceTests
{
    [Collection("AccountTests")]
    public class LogoutTests : AccountServiceTestsData
    {
        // Тестирование выхода из существующего аккаунта.
        [Fact]
        public void LogoutFromLoggedAccount()
        {
            State.loggedIn = new();

            var registerRequest = new RegisterRequest()
            {
                Email = "logoutAfterLogin@pochta.test",
                Password = "password",
                VerifyPassword = "password"
            };

            var loginRequest = new LoginRequest()
            {
                Email = registerRequest.Email,
                Password = registerRequest.Password
            };

            // Последовательная регистрация, вход в аккаунт и выход из него.
            var reply = service.Register(registerRequest, null).
                ContinueWith(login => service.Login(loginRequest, null)).
                ContinueWith(logout => service.Logout(
                    new SessionRequest { SessionId = logout.Result.Result.SessionId }, null));

            // Ожидается, что в результате будет успешный выход из аккаунта.
            Assert.Equal(ActionCode.Successful, reply.Result.Result.Result);
        }

        // Тестирование выхода из несуществующего аккаунта.
        [Fact]
        public void LogoutFromNotLoggedAccount()
        {
            var request = new SessionRequest { SessionId = "non_existing_sessionId" };
            var reply = service.Logout(request, null);
            // Ожадиается, что придет сообщение о том, что пользователь уже вышел из данного аккаунта.
            Assert.Equal(ActionCode.AccountNotFound, reply.Result.Result);
        }
    }
}
