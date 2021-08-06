using AccountGRPC.Validation;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.ValidationTests
{
    [Collection("AccountTests")]
    public class RegisterFieldsTests
    {
        // Тест проверки на то, какой результат вернет валидация, если есть пустые поля.
        [Theory]
        [InlineData("text", "password", "password", false)]
        [InlineData("text", "", "password", true)]
        [InlineData("", "", "", true)]
        public void EmptyRegisterFieldsTest(string email, string password, string verifyPassword, bool isEmpty)
        {
            // Валидация сразу же формируемого запроса.
            var reply = Validate.RegisterFields(new RegisterRequest {
                Email = email,
                Password = password,
                VerifyPassword = verifyPassword
            });
            // Если указано, что входные параметры являются пустыми, ожидается, что результатом валидации будет EmptyField.
            if (isEmpty) Assert.Equal(ActionCode.EmptyField, reply.Code);
            // Иначе ожидается, что результатом валидации будет любой другой ответ кроме EmptyField.
            else Assert.NotEqual(ActionCode.EmptyField, reply.Code);
        }

        // Тест проверки на то, являются ли введенные данные в поле Email электронной почтой.
        [Theory]
        [InlineData("pochta@mail.ru", true)]
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
            if (isEmail) Assert.NotEqual(ActionCode.IsNotEmail, reply.Code);
            // В ином случае ожидается ответ, что данные не являются электронной почтой.
            else Assert.Equal(ActionCode.IsNotEmail, reply.Code);
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
                Assert.NotEqual(ActionCode.PasswordMismatch, reply.Code);
            // В ином случае ожидается ответ PasswordMismatch (пароли не совпадают).
            else Assert.Equal(ActionCode.PasswordMismatch, reply.Code);
        }
    }
}
