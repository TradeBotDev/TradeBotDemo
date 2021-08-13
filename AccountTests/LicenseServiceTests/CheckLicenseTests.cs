﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}