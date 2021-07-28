using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.AccountServiceTests
{
    public class RegisterTests : AccountServiceTestsData
    {
        // Тестирование на работу регистрации нового аккаунта.
        [Fact]
        public void RegisterNewAccountTest()
        {
            var request = new RegisterRequest
            {
                Email = $"register_new_account{random.Next(0, 10000)}@pochta.test",
                Password = "password",
                VerifyPassword = "password"
            };
            var reply = service.Register(request, null);
            // Ожидается, что регистрация будет завершена успешно.
            Assert.Equal(ActionCode.Successful, reply.Result.Result);
        }
        
        // Тестирование на работу попытки регистрации существующего аккаунта.
        [Fact]
        public void RegisterExistingAccount()
        {
            var request = new RegisterRequest
            {
                Email = "register_existing_account@pochta.test",
                Password = "password",
                VerifyPassword = "password"
            };
            // Двойная регистрация аккаунта.
            service.Register(request, null);
            var reply = service.Register(request, null);

            // Ожидается, что придет сообщение о том, что такой аккаунт уже зарегистрирован.
            Assert.Equal(ActionCode.AccountExists, reply.Result.Result);
        }
    }
}
