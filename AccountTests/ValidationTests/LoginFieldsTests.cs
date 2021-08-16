using AccountGRPC.Validation;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.ValidationTests
{
    [Collection("AccountTests")]
    public class LoginFieldsTests
    {
        // Тест проверки на успешный исход валидации при правильно введенных полях.
        [Fact]
        public void CorrectLoginFieldsTest()
        {
            var reply = Validate.LoginFields(new LoginRequest { Email = "pochta@mail.test", Password = "password" });
            // Ожидается, что ответом будет true.
            Assert.True(reply.Successful);
        }

        // Тест проверки на то, какой результат вернет валидация, если есть пустые поля.
        [Theory]
        [InlineData("", "")]
        [InlineData("", "text")]
        [InlineData("text", "")]
        public void EmptyLoginFieldsTest(string email, string password)
        {
            // Валидация сразу же формируемого запроса.
            var reply = Validate.LoginFields(new LoginRequest { Email = email, Password = password } );
            // Ожидается, что ответом будет false.
            Assert.False(reply.Successful);
        }

        // Тест проверки на то, являются ли введенные данные в поле Email электронной почтой.
        [Theory]
        [InlineData("pochta@mail.ru", true)]
        [InlineData("a@a.a", true)]
        [InlineData("text", false)]
        public void NotEmailInLoginTest(string email, bool isEmail)
        {
            // Валидация сразу же формируемого запроса.
            var reply = Validate.LoginFields(new LoginRequest { Email = email, Password = "password"} );

            // В случае, если указано, что это именно электронная почта, ожидается, что результатом валидации
            // не будет false (не электронная почта).
            if (isEmail) Assert.True(reply.Successful);

            // В ином случае ожидается ответ, что данные не являются электронной почтой.
            else Assert.False(reply.Successful);
        }
    }
}
