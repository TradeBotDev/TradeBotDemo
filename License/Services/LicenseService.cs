using Grpc.Core;
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
                bool licenseIsExists = database.Licenses.Any(license => license.AccountId == request.AccountId);
                
                if (licenseIsExists)
                    return Task.FromResult(new SetLicenseResponse
                    {
                        Code = LicenseCode.IsExists,
                        Message = "Произошла ошибка добавления лицензии: лицензия уже существует на данный продукт."
                    });

                var license = new Models.License
                {
                    AccountId = request.AccountId,
                    Key = Guid.NewGuid().ToString(),
                    Product = request.Product
                };

                database.Licenses.Add(license);
                database.SaveChanges();

                return Task.FromResult(new SetLicenseResponse
                {
                    Code = LicenseCode.Successful,
                    Message = "Успешное добавление лицензии."
                });
            }
        }

        public override Task<LicenseCheckResponse> LicenseCheck(LicenseCheckRequest request, ServerCallContext context)
        {
            return base.LicenseCheck(request, context);
        }
    }
}
