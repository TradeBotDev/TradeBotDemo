using AccountGRPC;
using System;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace AccountTests.ExchangeAccessServiceTests
{
    // Класс, содержащий все необходимые поля для тестирования сервиса ExchangeAccessService.
    public abstract class ExchangeAccessServiceTestsData
    {
        // Объект сервиса системы аккаунтов для того, чтобы взаимодействовать с методами.
        public AccountService accountService = new();

        // Объект сервиса с доступами к бирже для того, чтобы взаимодействовать с методами.
        public ExchangeAccessService exchangeAccessService = new();

        // Метод, создающий временный аккаунт в процессе тестирования.
        public Task<Task<LoginReply>> GenerateLogin(string prefix)
        {
            // Запрос для регистрации.
            var registerRequest = new RegisterRequest
            {
                Email = $"{prefix}_generated_user@pochta.ru",
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
            return accountService.Register(registerRequest, null)
                .ContinueWith(registerReply => accountService.Login(loginRequest, null));
        }
    }
}
