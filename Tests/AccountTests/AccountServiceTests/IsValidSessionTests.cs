using AccountGRPC.Models;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.AccountServiceTests
{
    public class IsValidSessionTests : AccountServiceTestsData
    {
        // Тестирование проверки существующего вошедшего аккаунта на валидность.
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

            // Последовательная регистрация, вход в аккаунт и проверка валидности текущего входа.
            var reply = service.Register(registerRequest, null)
            .ContinueWith(login => service.Login(loginRequest, null))
            .ContinueWith(login => service.IsValidSession(
                new SessionRequest { SessionId = login.Result.Result.SessionId }, null));

            // Ожидается, что текущий вход будет являться валидным.
            Assert.True(reply.Result.Result.IsValid);
        }

        // Тестирование проверки невошедшего аккаунта на валидность.
        [Fact]
        public void NonExistingAccountIsValidTest()
        {
            // Намеренно отправляется несуществующий id сессии, чтобы аккаунт не был найден.
            var request = new SessionRequest { SessionId = "non_existing_session_id" };
            State.loggedIn = new();
            var reply = service.IsValidSession(request, null);

            // Ожидается, что придет сообщенение о том, что текущий вход не является валидным.
            Assert.False(reply.Result.IsValid);
        }
    }
}
