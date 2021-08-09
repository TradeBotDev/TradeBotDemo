﻿using AccountGRPC.LicenseMessages;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace AccountGRPC
{
    public class LicenseService : License.LicenseBase
    {
        public override Task<SetLicenseResponse> SetLicense(SetLicenseRequest request, ServerCallContext context)
        {
            using (var database = new Models.AccountContext())
            {
                var account = database.LoggedAccounts.Where(login => login.SessionId == request.SessionId);
                if (account.Count() == 0)
                    return Task.FromResult(SetLicenseReplies.AccountNotFound());

                bool isExists = database.Licenses.Any(license => license.Product == request.Product &&
                    license.Account.AccountId == account.First().AccountId);
                if (isExists)
                    return Task.FromResult(SetLicenseReplies.LicenseIsExists());

                else
                {
                    var license = new Models.License
                    {
                        Product = request.Product
                    };
                    account.First().Account.Licenses.Add(license);
                    database.SaveChanges();

                    return Task.FromResult(SetLicenseReplies.SuccessfulSettingLicense());
                }
            }
        }

        public override Task<CheckLicenseResponse> CheckLicense(CheckLicenseRequest request, ServerCallContext context)
        {
            return base.CheckLicense(request, context);
        }
    }
}