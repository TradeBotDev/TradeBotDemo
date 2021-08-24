using AccountGRPC;
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

        public LicenseService licenseService = new();

        // Метод, создающий временный аккаунт в процессе тестирования.
        public async Task<LoginResponse> GenerateLogin(string prefix)
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

            // Запрос для получения лицензии.
            var licenseRequest = new SetLicenseRequest
            {
                CardNumber = "1234123412341234",
                Cvv = 123,
                Date = 1234,
                Product = ProductCode.Tradebot
            };

            // Последовательно производится регистрация аккаунта, а затем вход в него, чтобы получить id
            // сессии и работать с ним.
            var reply = await accountService.Register(registerRequest, null)
                .ContinueWith(async registerReply => await accountService.Login(loginRequest, null));

            // Получение лицензии для того, чтобы была возможность получать биржи.
            licenseRequest.SessionId = reply.Result.SessionId;
            await licenseService.SetLicense(licenseRequest, null);

            // Возврат результата входа.
            return reply.Result;
        }
    }
}
