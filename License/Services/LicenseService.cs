using Grpc.Core;
using LicenseGRPC.LicenseMessages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.License.LicenseService.v1;

namespace LicenseGRPC
{
    public class LicenseService : License.LicenseBase
    {
        public override Task<SetLicenseResponse> SetLicense(SetLicenseRequest request, ServerCallContext context)
        {
            using (var database = new Models.LicenseContext())
            {
                bool licenseIsExists = database.Licenses.Any(license => license.AccountId == request.AccountId &&
                    license.Product == request.Product);

                if (licenseIsExists)
                    return Task.FromResult(SetLicenseReplies.LicenseIsExists());
                else
                {
                    var license = new Models.License
                    {
                        AccountId = request.AccountId,
                        Key = Guid.NewGuid().ToString(),
                        Product = request.Product
                    };

                    database.Licenses.Add(license);
                    database.SaveChanges();
                    return Task.FromResult(SetLicenseReplies.SuccessfulSettingLicense());
                }
            }
        }

        public override Task<LicenseCheckResponse> LicenseCheck(LicenseCheckRequest request, ServerCallContext context)
        {
            using (var database = new Models.LicenseContext())
            {
                bool isExists = database.Licenses.Any(license =>
                    license.AccountId == request.AccountId &&
                    license.Key == request.Key &&
                    license.Product == request.Product);

                if (isExists)
                    return Task.FromResult(LicenseCheckReplies.LicenseIsExists());
                else return Task.FromResult(LicenseCheckReplies.LicenseIsNotExists());
            }
        }

        public override Task<GetKeyResponse> GetKey(GetKeyRequest request, ServerCallContext context)
        {
            return base.GetKey(request, context);
        }
    }
}
