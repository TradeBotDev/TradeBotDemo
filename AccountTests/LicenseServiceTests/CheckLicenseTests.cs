using TradeBot.Account.AccountService.v1;
using Xunit;

namespace AccountTests.LicenseServiceTests
{
    [Collection("AccountTests")]
    public class CheckLicenseTests : LicenseServiceTestsData
    {
        [Fact]
        public void CheckLicenseFromNonExistingAccount()
        {
            var reply = licenseService.CheckLicense(new CheckLicenseRequest
            {
                SessionId = "not_existing_session_id",
                Product = ProductCode.Tradebot
            }, null);

            Assert.Equal(LicenseCode.NoAccess, reply.Result.Code);
        }

        [Fact]
        public void CheckNotExistingLicense()
        {
            var reply = GenerateLogin("not_ex_license").ContinueWith(login =>
                licenseService.CheckLicense(new CheckLicenseRequest
                {
                    SessionId = login.Result.Result.SessionId,
                    Product = ProductCode.Tradebot
                }, null));

            Assert.Equal(LicenseCode.NoAccess, reply.Result.Result.Code);
        }

        [Fact]
        public void CheckExistingLicense()
        {
            string session_id = null;
            string WriteSessionId(string _session_id)
            {
                session_id = _session_id;
                return _session_id;
            }

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

            Assert.Equal(LicenseCode.HaveAccess, reply.Result.Result.Code);
        }
    }
}
