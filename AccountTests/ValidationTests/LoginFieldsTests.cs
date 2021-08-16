using AccountGRPC.Validation;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.ValidationTests
{
    [Collection("AccountTests")]
    public class LoginFieldsTests
    {
        // ���� �������� �� �������� ����� ��������� ��� ��������� ��������� �����.
        [Fact]
        public void CorrectLoginFieldsTest()
        {
            var reply = Validate.LoginFields(new LoginRequest { Email = "pochta@mail.test", Password = "password" });
            // ���������, ��� ������� ����� true.
            Assert.True(reply.Successful);
        }

        // ���� �������� �� ��, ����� ��������� ������ ���������, ���� ���� ������ ����.
        [Theory]
        [InlineData("", "")]
        [InlineData("", "text")]
        [InlineData("text", "")]
        public void EmptyLoginFieldsTest(string email, string password)
        {
            // ��������� ����� �� ������������ �������.
            var reply = Validate.LoginFields(new LoginRequest { Email = email, Password = password } );
            // ���������, ��� ������� ����� false.
            Assert.False(reply.Successful);
        }

        // ���� �������� �� ��, �������� �� ��������� ������ � ���� Email ����������� ������.
        [Theory]
        [InlineData("pochta@mail.ru", true)]
        [InlineData("a@a.a", true)]
        [InlineData("text", false)]
        public void NotEmailInLoginTest(string email, bool isEmail)
        {
            // ��������� ����� �� ������������ �������.
            var reply = Validate.LoginFields(new LoginRequest { Email = email, Password = "password"} );

            // � ������, ���� �������, ��� ��� ������ ����������� �����, ���������, ��� ����������� ���������
            // �� ����� false (�� ����������� �����).
            if (isEmail) Assert.True(reply.Successful);

            // � ���� ������ ��������� �����, ��� ������ �� �������� ����������� ������.
            else Assert.False(reply.Successful);
        }
    }
}
