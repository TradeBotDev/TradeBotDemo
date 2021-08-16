using AccountGRPC.Validation;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.ValidationTests
{
    [Collection("AccountTests")]
    public class RegisterFieldsTests
    {
        // Тест проверки на успешный исход валидации при правильно введенных полях.
        [Fact]
        public void CorrectRegisterFieldsTest()
        {
            var reply = Validate.RegisterFields(new RegisterRequest
            {
                Email = "pochta@mail.test",
                Password = "password",
                VerifyPassword = "password"
            });

            Assert.True(reply.Successful);
        }

        // Тест проверки на то, какой результат вернет валидация, если есть пустые поля.
        [Theory]
        [InlineData("pochta@mail.test", "", "password")]
        [InlineData("", "", "")]
        public void EmptyRegisterFieldsTest(string email, string password, string verifyPassword)
        {
            // Валидация сразу же формируемого запроса.
            var reply = Validate.RegisterFields(new RegisterRequest {
                Email = email,
                Password = password,
                VerifyPassword = verifyPassword
            });

            // Ожидается, что результатом валидации будет false.
            Assert.False(reply.Successful);
        }

        // Тест проверки на то, являются ли введенные данные в поле Email электронной почтой.
        [Theory]
        [InlineData("pochta@mail.test", true)]
        [InlineData("a@a.a", true)]
        [InlineData("text", false)]
        public void NotEmailInRegisterTest(string email, bool isEmail)
        {
            // Валидация сразу же формируемого запроса.
            var reply = Validate.RegisterFields(new RegisterRequest {
                Email = email,
                Password = "psw",
                VerifyPassword = "psw"}
            );
            // В случае, если указано, что это именно электронная почта, ожидается, что результатом валидации
            // не будет IsNotEmail (не электронная почта).
            if (isEmail) Assert.True(reply.Successful);
            // В ином случае ожидается ответ, что данные не являются электронной почтой.
            else Assert.False(reply.Successful);
        }

        // Тест проверки на то, совпадают ли введенные пароли в запросе.
        [Theory]
        [InlineData("password", "password")]
        [InlineData("password", "other_password")]
        public void PasswordMismatchInRegisterTest(string password, string verifyPassword)
        {
            // Валидация сразу же формируемого запроса.
            var reply = Validate.RegisterFields(new RegisterRequest {
                Email = "pochta@mail.ru",
                Password = password,
                VerifyPassword = verifyPassword}
            );
            // В случае, если пароли совпадают, ожидается любой ответ, кроме PasswordMismatch (пароли не совпадают).
            if (password == verifyPassword)
                Assert.True(reply.Successful);
            // В ином случае ожидается ответ PasswordMismatch (пароли не совпадают).
            else Assert.False(reply.Successful);
        }
    }
}
