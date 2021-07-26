using Account.Validation;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.ValidationTests
{
    public class LoginFieldsTests
    {
        // Тест проверки на то, какой результат вернет валидация, если есть пустые поля.
        [Theory]
        [InlineData("", "")]
        [InlineData("", "text")]
        [InlineData("text", "")]
        public void EmptyLoginFieldsTest(string email, string password)
        {
            // Валидация сразу же формируемого запроса.
            var reply = Validate.LoginFields(new LoginRequest { Email = email, Password = password } );
            // Ожидается, что Code будет равен EmptyField.
            Assert.Equal(ActionCode.EmptyField, reply.Code);
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
            // не будет IsNotEmail (не электронная почта).
            if (isEmail) Assert.NotEqual(ActionCode.IsNotEmail, reply.Code);
            // В ином случае ожидается ответ, что данные не являются электронной почтой.
            else Assert.Equal(ActionCode.IsNotEmail, reply.Code);
        }
    }
}
