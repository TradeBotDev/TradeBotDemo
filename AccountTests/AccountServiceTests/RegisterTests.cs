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
                Email = $"user{random.Next(0, 10000)}@pochta.test",
                Password = "password",
                VerifyPassword = "password"
            };
            
            var reply = service.Register(request, null);
            Assert.Equal(ActionCode.Successful, reply.Result.Result);
        }
        
        // Тестирование на работу попытки регистрации существующего аккаунта.
        [Fact]
        public void RegisterExistingAccount()
        {
            var request = new RegisterRequest
            {
                Email = "user@pochta.test",
                Password = "password",
                VerifyPassword = "password"
            };

            service.Register(request, null);
            var reply = service.Register(request, null);
            Assert.Equal(ActionCode.AccountExists, reply.Result.Result);
        }
    }
}
