using System.Linq;
using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.LicenseServiceTests
{
    [Collection("AccountTests")]
    public class SetLicenseTests : LicenseServiceTestsData
    {
        // Проверка добавления лицензии в несуществующий аккаунт.
        [Fact]
        public void SetLicenseToNotExistingAccount()
        {
            // Запрос на установку лицензии.
            var reply = licenseService.SetLicense(new SetLicenseRequest
            {
                SessionId = "not_existing_session_id",
                CardNumber = "1234567812345678",
                Product = ProductCode.Tradebot,
                Date = 1234,
                Cvv = 123
            }, null);
            // Ожидается, что пользователь не будет иметь доступа к лицензии, потому что
            // аккаунт не будет существовать.
            Assert.Equal(LicenseCode.NoAccess, reply.Result.Code);
        }

        [Fact]
        public async void SetLicenseToExistingAccount()
        {
            // Генерация аккаунта и добавление в него лицензии.
            var reply = await GenerateLogin("set_lic_to_ex").ContinueWith(async login =>
                await licenseService.SetLicense(new SetLicenseRequest
                {
                    SessionId = login.Result.SessionId,
                    CardNumber = "1234123412341234",
                    Product = ProductCode.Unspecified,
                    Date = 1234,
                    Cvv = 123
                }, null));

            using (var database = new AccountGRPC.Models.AccountContext())
            {
                var licenses = database.Licenses.Where(license => license.Product == ProductCode.Unspecified);
                database.Licenses.Remove(licenses.First());
                database.SaveChanges();
            }

            // Ожидается, что добавление лицензии будет успешным.
            Assert.Equal(LicenseCode.Successful, reply.Result.Code);
        }
    }
}
