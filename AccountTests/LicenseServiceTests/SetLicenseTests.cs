using TradeBot.Account.AccountService.v1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AccountTests.LicenseServiceTests
{
    [Collection("AccountTests")]
    public class SetLicenseTests : LicenseServiceTestsData
    {
        [Fact]
        public void SetLicenseToNotExistingAccount()
        {
            var reply = licenseService.SetLicense(new SetLicenseRequest
            {
                SessionId = "not_existing_session_id",
                CardNumber = "1234567812345678",
                Product = ProductCode.Tradebot,
                Date = 1234,
                Cvv = 123
            }, null);

            Assert.Equal(LicenseCode.NoAccess, reply.Result.Code);
        }
    }
}
