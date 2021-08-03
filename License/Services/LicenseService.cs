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

        public override Task<LicenseCheckResponse> LicenseCheck(LicenseCheckRequest request, ServerCallContext context)
        {
            return base.LicenseCheck(request, context);
        }

        public override Task<GetKeyResponse> GetKey(GetKeyRequest request, ServerCallContext context)
        {
            return base.GetKey(request, context);
        }
    }
}
