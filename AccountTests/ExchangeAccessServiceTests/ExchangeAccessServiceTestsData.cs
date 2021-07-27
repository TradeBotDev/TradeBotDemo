using AccountGRPC;
using AccountGRPC.Models;
using System;
using TradeBot.Account.AccountService.v1;

namespace AccountTests.ExchangeAccessServiceTests
{
    // Класс, содержащий все необходимые поля для тестирования сервиса ExchangeAccessService.
    public abstract class ExchangeAccessServiceTestsData
    {
        public Random random = new();

        // Объект сервиса системы аккаунтов для того, чтобы взаимодействовать с методами.
        public AccountService accountService = new();

        // Объект сервиса с доступами к бирже для того, чтобы взаимодействовать с методами.
        public ExchangeAccessService exchangeAccessService = new();

        // Метод, создающий временный аккаунт в процессе тестирования.
        public string GenerateLogin(bool withExchangeAccesses)
        {
            // Запрос для регистрации.
            var registerRequest = new RegisterRequest
            {
                Email = $"generated_user{random.Next(1, 1000000)}@pochta.ru",
                Password = "test_pawsword",
                VerifyPassword = "test_pawsword"
            };

            // Запрос для входа в аккаунт.
            var loginRequest = new LoginRequest
            {
                Email = registerRequest.Email,
                Password = registerRequest.Password
            };

            // Последовательно производится регистрация аккаунта, а затем вход в него, чтобы получить id
            // сессии и работать с ним.
            string sessionId = accountService.Register(registerRequest, null)
                .ContinueWith(registerReply => accountService.Login(loginRequest, null))
                .Result.Result.SessionId;

            return sessionId;
        }
    }
}
