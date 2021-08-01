using AccountGRPC.Validation;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.ValidationTests
{
    [Collection("AccountTests")]
    public class AddExchangeAccessFieldsTests
    {
        [Fact]
        public void CorrectExchangeFieldsTest()
        {
            var reply = Validate.AddExchangeAccessFields(new AddExchangeAccessRequest
            {
                SessionId = "not_existing_session_id",
                ExchangeName = "Bitmex",
                Token = "not_existing_token",
                Secret = "not_existing_secret"
            });
            // Ожидается, что в результате ответом будет true.
            Assert.True(reply.Successful);
        }

        // Тест проверки на то, какой результат вернет валидация, если есть пустые поля.
        [Theory]
        [InlineData("text", "text", "", "text")]
        [InlineData("", "", "", "")]
        public void EmptyExchangeFieldsTest(string sessionId, string exchangeName, string token, string secret)
        {
            // Валидация сразу же формируемого запроса.
            var reply = Validate.AddExchangeAccessFields(new AddExchangeAccessRequest
            {
                SessionId = sessionId,
                ExchangeName = exchangeName,
                Token = token,
                Secret = secret
            });
            // Ожидается, что в результате ответом будет false.
            Assert.False(reply.Successful);
        }
    }
}
