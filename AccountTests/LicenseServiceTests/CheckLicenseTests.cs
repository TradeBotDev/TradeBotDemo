using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.LicenseServiceTests
{
    [Collection("AccountTests")]
    public class CheckLicenseTests : LicenseServiceTestsData
    {
        // Тестирование проверки лицензии из несуществующего аккаунта.
        [Fact]
        public void CheckLicenseFromNonExistingAccount()
        {
            // Отправка запроса с несуществующим Id сессии.
            var reply = licenseService.CheckLicense(new CheckLicenseRequest
            {
                SessionId = "not_existing_session_id",
                Product = ProductCode.Tradebot
            }, null);
            // Ожидается, что у пользователя не будет доступа к продукту.
            Assert.Equal(LicenseCode.NoAccess, reply.Result.Code);
        }

        // Тестирование проверки несуществующей лицензии из аккаунта.
        [Fact]
        public void CheckNotExistingLicense()
        {
            // Генерация аккаунта и отправка запроса с существующим Id сессии.
            var reply = GenerateLogin("not_ex_license").ContinueWith(login =>
                licenseService.CheckLicense(new CheckLicenseRequest
                {
                    SessionId = login.Result.Result.SessionId,
                    Product = ProductCode.Tradebot
                }, null));
            // Ожидается, что у пользователя по прежнему не будет доступа к продукту.
            Assert.Equal(LicenseCode.NoAccess, reply.Result.Result.Code);
        }

        [Fact]
        public void CheckExistingLicense()
        {
            string session_id = null;
            // Метод для записи Id сессии в переменную, чтобы ее можно было использовать в дальнейшем.
            string WriteSessionId(string _session_id)
            {
                session_id = _session_id;
                return _session_id;
            }

            // Последовательная генерация входа, установка лицензии, а затем проверка лицензии.
            var reply = GenerateLogin("ex_license").ContinueWith(login =>
               licenseService.SetLicense(new SetLicenseRequest
               {
                   SessionId = WriteSessionId(login.Result.Result.SessionId),
                   Product = ProductCode.Tradebot,
                   CardNumber = "1234567812345678",
                   Date = 1234,
                   Cvv = 123
               }, null)).ContinueWith(set =>
                    licenseService.CheckLicense(new CheckLicenseRequest
                    {
                        SessionId = session_id,
                        Product = ProductCode.Tradebot
                    }, null));
            // Ожидается, что пользователь будет иметь доступ к лицензии.
            Assert.Equal(LicenseCode.HaveAccess, reply.Result.Result.Code);
        }
    }
}
