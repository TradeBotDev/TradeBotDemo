using AccountGRPC.Validation;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.ValidationTests
{
    [Collection("AccountTests")]
    public class AddExchangeAccessFieldsTests
    {
        // Тест проверки на то, какой результат вернет валидация, если есть пустые поля.
        [Theory]
        [InlineData("text", "text", "text", "text", false)]
        [InlineData("text", "text", "", "text", true)]
        [InlineData("", "", "", "", true)]
        public void EmptyExchangeFieldsTest(string sessionId, string exchangeName, string token, string secret, bool isEmpty)
        {
            // Валидация сразу же формируемого запроса.
            var reply = Validate.AddExchangeAccessFields(new AddExchangeAccessRequest
            {
                SessionId = sessionId,
                ExchangeName = exchangeName,
                Token = token,
                Secret = secret
            });

            // Если указано, что присутствуют пустые поля, ожидается, что в результате ответом будет EmptyField.
            if (isEmpty) Assert.Equal(ActionCode.EmptyField, reply.Code);
            // Иначе ожидается, что будет любой другой ответ, кроме EmptyField.
            else Assert.NotEqual(ActionCode.EmptyField, reply.Code);
        }
    }
}
