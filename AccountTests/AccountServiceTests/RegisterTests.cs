using System.Linq;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.AccountServiceTests
{
    [Collection("AccountTests")]
    public class RegisterTests : AccountServiceTestsData
    {
        // Тестирование на работу регистрации нового аккаунта.
        [Fact]
        public void RegisterNewAccountTest()
        {
            var request = new RegisterRequest
            {
                Email = $"register_new_account@pochta.test",
                Password = "password",
                VerifyPassword = "password"
            };
            var reply = service.Register(request, null);

            using (var database = new AccountGRPC.Models.AccountContext())
            {
                var accounts = database.Accounts.Where(account => account.Email == request.Email);
                database.Accounts.Remove(accounts.First());
                database.SaveChanges();
            }

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

            using (var database = new AccountGRPC.Models.AccountContext())
            {
                var accounts = database.Accounts.Where(account => account.Email == request.Email);
                database.Accounts.Remove(accounts.First());
                database.SaveChanges();
            }

            // Ожидается, что придет сообщение о том, что такой аккаунт уже зарегистрирован.
            Assert.Equal(ActionCode.AccountExists, reply.Result.Result);
        }
    }
}
